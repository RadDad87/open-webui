using Z3R4H.Wrapper.Config;
using Z3R4H.Wrapper.Fallback;
using Z3R4H.Wrapper.Logging;
using Z3R4H.Wrapper.Orchestration;

namespace Z3R4H.Wrapper.App;

public sealed class WrapperApp(
	WrapperLogger logger,
	ConfigResolver configResolver,
	RuntimeOrchestrator orchestrator,
	BatFallback batFallback)
{
	public async Task<StartupResult> RunAsync(string[] args, CancellationToken ct = default)
	{
		try
		{
			logger.Stage("startup", "Task 25 wrapper milestone starting");
			WrapperConfig config;
			try
			{
				config = configResolver.Resolve(args);
			}
			catch (Exception ex)
			{
				logger.Error(WrapperErrorCategory.ConfigError, ex.Message);
				return new StartupResult(false, (int)WrapperExitCode.ConfigResolutionFailed, ex.Message, WrapperErrorCategory.ConfigError, "config");
			}

			logger.Stage("config", $"Resolved package root ({config.PackageRootSource}): {config.PackageRoot}");
			logger.Info($"Resolved BAT path: {config.BatLauncherPath}");
			logger.Info($"Resolved llama path: {config.LlamaServerExePath}");
			logger.Info($"StartupMode={config.StartupMode}; FallbackMode={config.FallbackMode}; WebUI={config.OpenWebUiStartCommand}");

			if (string.Equals(config.FallbackMode, "always", StringComparison.OrdinalIgnoreCase))
			{
				var fallbackAlways = batFallback.InvokeFallback(config, "Fallback mode is set to always");
				return fallbackAlways.Success
					? new StartupResult(true, (int)WrapperExitCode.FallbackInvoked, fallbackAlways.Message, Stage: "fallback")
					: new StartupResult(false, (int)WrapperExitCode.FallbackInvocationFailed, fallbackAlways.Message, WrapperErrorCategory.FallbackInvocation, "fallback");
			}

			var orchestrationResult = await orchestrator.StartAsync(config, ct);
			if (!orchestrationResult.Success)
			{
				logger.Error(orchestrationResult.Category ?? WrapperErrorCategory.StartupFailure, orchestrationResult.Message);

				if (string.Equals(config.FallbackMode, "auto", StringComparison.OrdinalIgnoreCase)
					&& !string.Equals(orchestrationResult.Stage, "browser-launch", StringComparison.OrdinalIgnoreCase))
				{
					var fallback = batFallback.InvokeFallback(config, $"{orchestrationResult.Stage}: {orchestrationResult.Message}");
					return fallback.Success
						? new StartupResult(true, (int)WrapperExitCode.FallbackInvoked, fallback.Message, Stage: "fallback")
						: new StartupResult(false, (int)WrapperExitCode.FallbackInvocationFailed, fallback.Message, WrapperErrorCategory.FallbackInvocation, "fallback");
				}

				var exitCode = orchestrationResult.Stage switch
				{
					"llama-launch" => (int)WrapperExitCode.LlamaLaunchFailed,
					"llama-readiness" => (int)WrapperExitCode.LlamaReadinessFailed,
					"webui-launch" => (int)WrapperExitCode.WebUiLaunchFailed,
					"webui-readiness" => (int)WrapperExitCode.WebUiReadinessFailed,
					"browser-launch" => (int)WrapperExitCode.BrowserLaunchFailed,
					_ when orchestrationResult.Category == WrapperErrorCategory.ModeError => (int)WrapperExitCode.UnsupportedMode,
					_ => (int)WrapperExitCode.ConfigValidationFailed
				};

				return new StartupResult(false, exitCode, orchestrationResult.Message, orchestrationResult.Category, orchestrationResult.Stage);
			}

			logger.Stage("startup", "Task 25 milestone completed (dual-service readiness + browser launch request)");
			return new StartupResult(true, (int)WrapperExitCode.Success, orchestrationResult.Message, Stage: orchestrationResult.Stage);
		}
		catch (Exception ex)
		{
			logger.Error(WrapperErrorCategory.UnhandledError, ex.Message);
			return new StartupResult(false, (int)WrapperExitCode.UnhandledError, ex.Message, WrapperErrorCategory.UnhandledError, "unhandled");
		}
	}
}
