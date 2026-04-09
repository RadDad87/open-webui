using System.Diagnostics;
using Z3R4H.Wrapper.Config;
using Z3R4H.Wrapper.Health;
using Z3R4H.Wrapper.Logging;
using Z3R4H.Wrapper.UI;

namespace Z3R4H.Wrapper.Orchestration;

public sealed class RuntimeOrchestrator(WrapperLogger logger, ReadinessProbe readinessProbe, StartupScreen startupScreen)
{
	public async Task<(bool Success, WrapperErrorCategory? Category, string Stage, string Message)> StartAsync(WrapperConfig config, CancellationToken ct = default)
	{
		startupScreen.ShowInitializing();
		logger.Stage("orchestration", "Z3R4H single-click startup sequence beginning");

		var validation = ValidateConfig(config);
		if (!validation.Success)
		{
			startupScreen.StepFailure("Configuration", validation.Message);
			return validation;
		}
		startupScreen.StepSuccess("Configuration validated");

		startupScreen.StepStart("AI service startup");
		var aiStart = await StartAiAsync(config, ct);
		if (!aiStart.Success)
		{
			startupScreen.StepFailure("AI service", aiStart.Message);
			return aiStart;
		}
		startupScreen.StepSuccess("AI service ready");

		startupScreen.StepStart("Backend startup");
		var backendStart = StartProcessCommand(config.BackendStartCommand, config.PackageRoot, "backend-launch", "backend");
		if (!backendStart.Success)
		{
			startupScreen.StepFailure("Backend", backendStart.Message);
			return backendStart;
		}

		var backendReady = await readinessProbe.WaitForBackendReadyAsync(config.BackendHealthUrl, config.BackendReadyTimeoutSeconds, config.BackendReadyPollSeconds, ct);
		if (!backendReady.Success)
		{
			startupScreen.StepFailure("Backend", backendReady.Message);
			return (false, WrapperErrorCategory.Timeout, "backend-readiness", backendReady.Message);
		}
		startupScreen.StepSuccess("Backend ready");

		startupScreen.StepStart("Routing engine startup");
		var routingReady = await EnsureRoutingReadyAsync(config, ct);
		var basicMode = !routingReady.Success;
		if (basicMode)
		{
			logger.Error(WrapperErrorCategory.ProbeFailure, routingReady.Message);
			startupScreen.StepWarning("Routing", "Routing unavailable; falling back to basic maps mode.");
		}
		else
		{
			startupScreen.StepSuccess("Routing ready");
		}

		var launchUrl = config.BrowserStartUrl;
		var browserLaunch = LaunchBrowser(launchUrl);
		if (!browserLaunch.Success)
		{
			startupScreen.StepFailure("Browser launch", browserLaunch.Message);
			return browserLaunch;
		}

		startupScreen.Complete(basicMode);
		var message = basicMode
			? "AI and backend ready; routing unavailable, launched browser in basic mode."
			: "AI, backend, and routing all ready; browser launch requested for /maps.";

		return (true, null, basicMode ? "browser-launch-basic" : "browser-launch", message);
	}

	private (bool Success, WrapperErrorCategory? Category, string Stage, string Message) ValidateConfig(WrapperConfig config)
	{
		if (!Directory.Exists(config.PackageRoot))
		{
			return (false, WrapperErrorCategory.PathError, "config", $"Package root not found: {config.PackageRoot}");
		}

		if (!File.Exists(config.BatLauncherPath))
		{
			return (false, WrapperErrorCategory.PathError, "config", $"BAT fallback launcher not found: {config.BatLauncherPath}");
		}

		if (!string.Equals(config.StartupMode, "exe-first", StringComparison.OrdinalIgnoreCase))
		{
			return (false, WrapperErrorCategory.ModeError, "config", $"Unsupported startup mode: {config.StartupMode}");
		}

		var fallbackModes = new[] { "auto", "always", "never" };
		if (!fallbackModes.Contains(config.FallbackMode, StringComparer.OrdinalIgnoreCase))
		{
			return (false, WrapperErrorCategory.ConfigError, "config", $"Invalid fallback mode: {config.FallbackMode}");
		}

		if (string.IsNullOrWhiteSpace(config.BackendStartCommand))
		{
			return (false, WrapperErrorCategory.ConfigError, "config", "Backend start command is empty.");
		}

		if (string.IsNullOrWhiteSpace(config.BrowserStartUrl))
		{
			return (false, WrapperErrorCategory.ConfigError, "config", "Browser start URL is empty.");
		}

		return (true, null, "config", "Configuration validation passed.");
	}

	private async Task<(bool Success, WrapperErrorCategory? Category, string Stage, string Message)> StartAiAsync(WrapperConfig config, CancellationToken ct)
	{
		var preferred = config.AiMode.ToLowerInvariant();
		var canLlama = File.Exists(config.LlamaServerExePath) && File.Exists(config.LlamaModelPath);
		var canOllama = File.Exists(config.OllamaExePath);

		if (preferred == "llama" || (preferred == "auto" && canLlama))
		{
			var llamaStart = StartLlama(config);
			if (!llamaStart.Success)
			{
				return llamaStart;
			}

			var endpoint = $"http://{config.LlamaHost}:{config.LlamaPort}/health";
			var ready = await readinessProbe.WaitForAiReadyAsync(endpoint, config.LlamaReadyTimeoutSeconds, config.LlamaReadyPollSeconds, ct);
			return ready.Success
				? (true, null, "ai-readiness", ready.Message)
				: (false, WrapperErrorCategory.Timeout, "ai-readiness", ready.Message);
		}

		if (preferred == "ollama" || (preferred == "auto" && canOllama))
		{
			var ollamaStart = StartProcess(config.OllamaExePath, "serve", Path.GetDirectoryName(config.OllamaExePath) ?? config.PackageRoot, "ai-launch", "ollama");
			if (!ollamaStart.Success)
			{
				return ollamaStart;
			}

			var endpoint = $"http://{config.OllamaHost}:{config.OllamaPort}/api/tags";
			var ready = await readinessProbe.WaitForAiReadyAsync(endpoint, config.OllamaReadyTimeoutSeconds, config.OllamaReadyPollSeconds, ct);
			return ready.Success
				? (true, null, "ai-readiness", ready.Message)
				: (false, WrapperErrorCategory.Timeout, "ai-readiness", ready.Message);
		}

		return (false, WrapperErrorCategory.PathError, "ai-launch", "No usable local AI runtime found (llama or ollama).");
	}

	private async Task<(bool Success, string Message)> EnsureRoutingReadyAsync(WrapperConfig config, CancellationToken ct)
	{
		if (!string.IsNullOrWhiteSpace(config.ValhallaStartCommand))
		{
			var valhallaStart = StartProcessCommand(config.ValhallaStartCommand, config.PackageRoot, "routing-launch", "valhalla");
			if (!valhallaStart.Success)
			{
				return (false, valhallaStart.Message);
			}
		}

		var valhallaReady = await readinessProbe.WaitForValhallaReadyAsync(config.ValhallaHealthUrl, config.ValhallaReadyTimeoutSeconds, config.ValhallaReadyPollSeconds, ct);
		if (!valhallaReady.Success)
		{
			return (false, valhallaReady.Message);
		}

		var geoReady = await readinessProbe.WaitForGeoHealthAsync(config.GeoHealthUrl, config.GeoHealthTimeoutSeconds, config.GeoHealthPollSeconds, ct);
		return geoReady;
	}

	private (bool Success, WrapperErrorCategory? Category, string Stage, string Message) StartLlama(WrapperConfig config)
	{
		if (!File.Exists(config.LlamaServerExePath))
		{
			return (false, WrapperErrorCategory.PathError, "ai-launch", $"llama executable not found: {config.LlamaServerExePath}");
		}
		if (!File.Exists(config.LlamaModelPath))
		{
			return (false, WrapperErrorCategory.PathError, "ai-launch", $"llama model not found: {config.LlamaModelPath}");
		}

		var args = $"-m \"{config.LlamaModelPath}\" --host {config.LlamaHost} --port {config.LlamaPort}";
		return StartProcess(config.LlamaServerExePath, args, Path.GetDirectoryName(config.LlamaServerExePath) ?? config.PackageRoot, "ai-launch", "llama");
	}

	private (bool Success, WrapperErrorCategory? Category, string Stage, string Message) StartProcessCommand(string command, string workingDirectory, string stage, string label)
	{
		if (string.IsNullOrWhiteSpace(command))
		{
			return (false, WrapperErrorCategory.ConfigError, stage, $"{label} command is empty.");
		}

		return StartProcess("cmd.exe", $"/c {command}", workingDirectory, stage, label);
	}

	private (bool Success, WrapperErrorCategory? Category, string Stage, string Message) StartProcess(string fileName, string arguments, string workingDirectory, string stage, string label)
	{
		try
		{
			var psi = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = arguments,
				WorkingDirectory = workingDirectory,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			var process = Process.Start(psi);
			if (process is null)
			{
				return (false, WrapperErrorCategory.StartupFailure, stage, $"Failed to start {label} process.");
			}

			logger.Stage("orchestration", $"{label} process started (pid={process.Id})");
			return (true, null, stage, $"{label} process started.");
		}
		catch (Exception ex)
		{
			return (false, WrapperErrorCategory.StartupFailure, stage, $"Exception starting {label}: {ex.Message}");
		}
	}

	private (bool Success, WrapperErrorCategory? Category, string Stage, string Message) LaunchBrowser(string url)
	{
		try
		{
			var psi = new ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			};

			var process = Process.Start(psi);
			if (process is null)
			{
				return (false, WrapperErrorCategory.StartupFailure, "browser-launch", $"Failed to launch default browser for URL: {url}");
			}

			logger.Stage("orchestration", $"browser launch requested for {url}");
			return (true, null, "browser-launch", "Browser launch requested.");
		}
		catch (Exception ex)
		{
			return (false, WrapperErrorCategory.StartupFailure, "browser-launch", $"Exception launching browser: {ex.Message}");
		}
	}
}
