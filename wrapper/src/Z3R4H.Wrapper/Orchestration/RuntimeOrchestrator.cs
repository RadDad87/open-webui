using System.Diagnostics;
using Z3R4H.Wrapper.Config;
using Z3R4H.Wrapper.Health;
using Z3R4H.Wrapper.Logging;

namespace Z3R4H.Wrapper.Orchestration;

public sealed class RuntimeOrchestrator(WrapperLogger logger, ReadinessProbe readinessProbe)
{
	public async Task<(bool Success, WrapperErrorCategory? Category, string Stage, string Message)> StartAsync(WrapperConfig config, CancellationToken ct = default)
	{
		logger.Stage("orchestration", "Task 22 milestone: dual-service startup (llama + open-webui readiness)");

		var validation = ValidateConfig(config);
		if (!validation.Success)
		{
			return validation;
		}

		var llamaStart = StartLlama(config);
		if (!llamaStart.Success)
		{
			return llamaStart;
		}

		var llamaEndpoint = $"http://{config.LlamaHost}:{config.LlamaPort}/health";
		var llamaReady = await readinessProbe.WaitForLlamaReadyAsync(llamaEndpoint, config.LlamaReadyTimeoutSeconds, config.LlamaReadyPollSeconds, ct);
		if (!llamaReady.Success)
		{
			return (false, WrapperErrorCategory.Timeout, "llama-readiness", llamaReady.Message);
		}

		var webuiStart = StartWebUi(config);
		if (!webuiStart.Success)
		{
			return webuiStart;
		}

		var webuiReady = await readinessProbe.WaitForWebUiReadyAsync(config.OpenWebUiUrl, config.OpenWebUiReadyTimeoutSeconds, config.OpenWebUiReadyPollSeconds, ct);
		if (!webuiReady.Success)
		{
			return (false, WrapperErrorCategory.Timeout, "webui-readiness", webuiReady.Message);
		}

		var browserLaunch = LaunchBrowser(config.OpenWebUiUrl);
		if (!browserLaunch.Success)
		{
			return browserLaunch;
		}

		return (true, null, "browser-launch", "llama and open-webui startup/readiness succeeded; browser launch requested.");
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

		if (!File.Exists(config.LlamaServerExePath))
		{
			return (false, WrapperErrorCategory.PathError, "config", $"llama executable not found: {config.LlamaServerExePath}");
		}

		if (!File.Exists(config.LlamaModelPath))
		{
			return (false, WrapperErrorCategory.PathError, "config", $"llama model not found: {config.LlamaModelPath}");
		}

		if (!string.Equals(config.StartupMode, "exe-first", StringComparison.OrdinalIgnoreCase))
		{
			return (false, WrapperErrorCategory.ModeError, "config", $"Unsupported startup mode for milestone: {config.StartupMode}");
		}

		var fallbackModes = new[] { "auto", "always", "never" };
		if (!fallbackModes.Contains(config.FallbackMode, StringComparer.OrdinalIgnoreCase))
		{
			return (false, WrapperErrorCategory.ConfigError, "config", $"Invalid fallback mode: {config.FallbackMode}");
		}

		if (string.IsNullOrWhiteSpace(config.OpenWebUiStartCommand))
		{
			return (false, WrapperErrorCategory.ConfigError, "config", "Open WebUI start command is empty.");
		}

		if (string.IsNullOrWhiteSpace(config.OpenWebUiUrl))
		{
			return (false, WrapperErrorCategory.ConfigError, "config", "Open WebUI URL is empty.");
		}

		return (true, null, "config", "Configuration validation passed.");
	}

	private (bool Success, WrapperErrorCategory? Category, string Stage, string Message) StartLlama(WrapperConfig config)
	{
		try
		{
			var psi = new ProcessStartInfo
			{
				FileName = config.LlamaServerExePath,
				Arguments = $"-m \"{config.LlamaModelPath}\" --host {config.LlamaHost} --port {config.LlamaPort}",
				WorkingDirectory = Path.GetDirectoryName(config.LlamaServerExePath) ?? config.PackageRoot,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			var process = Process.Start(psi);
			if (process is null)
			{
				return (false, WrapperErrorCategory.StartupFailure, "llama-launch", "Failed to start llama process.");
			}

			logger.Stage("orchestration", $"llama process started (pid={process.Id})");
			return (true, null, "llama-launch", "llama process started.");
		}
		catch (Exception ex)
		{
			return (false, WrapperErrorCategory.StartupFailure, "llama-launch", $"Exception starting llama: {ex.Message}");
		}
	}

	private (bool Success, WrapperErrorCategory? Category, string Stage, string Message) StartWebUi(WrapperConfig config)
	{
		try
		{
			var psi = new ProcessStartInfo
			{
				FileName = "cmd.exe",
				Arguments = $"/c {config.OpenWebUiStartCommand}",
				WorkingDirectory = config.PackageRoot,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			var process = Process.Start(psi);
			if (process is null)
			{
				return (false, WrapperErrorCategory.StartupFailure, "webui-launch", "Failed to start Open WebUI process.");
			}

			logger.Stage("orchestration", $"open-webui process started (pid={process.Id})");
			return (true, null, "webui-launch", "Open WebUI process started.");
		}
		catch (Exception ex)
		{
			return (false, WrapperErrorCategory.StartupFailure, "webui-launch", $"Exception starting Open WebUI: {ex.Message}");
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
