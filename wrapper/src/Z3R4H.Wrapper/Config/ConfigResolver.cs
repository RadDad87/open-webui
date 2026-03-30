using System.Reflection;

namespace Z3R4H.Wrapper.Config;

public sealed class ConfigResolver
{
	public WrapperConfig Resolve(string[] args)
	{
		var values = ParseArgs(args);
		var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
		var packageRoot = values.TryGetValue("package-root", out var configuredRoot)
			? Path.GetFullPath(configuredRoot)
			: Path.GetFullPath(Path.Combine(exeDir, "..", "..", ".."));

		var startupMode = values.GetValueOrDefault("startup-mode", "exe-first");
		var fallbackMode = values.GetValueOrDefault("fallback", "auto");

		var llamaExe = values.GetValueOrDefault(
			"llama-exe",
			Path.Combine(packageRoot, "runtime", "llama.cpp", "llama-server.exe"));
		var llamaModel = values.GetValueOrDefault(
			"llama-model",
			Path.Combine(packageRoot, "models", "model.gguf"));

		var llamaHost = values.GetValueOrDefault("llama-host", "127.0.0.1");
		var llamaPort = ParseInt(values.GetValueOrDefault("llama-port", "11434"), 11434);
		var llamaTimeout = ParseInt(values.GetValueOrDefault("llama-timeout", "120"), 120);
		var llamaPoll = ParseInt(values.GetValueOrDefault("llama-poll", "2"), 2);

		var webuiCmd = values.GetValueOrDefault("webui-cmd", "open-webui serve");
		var webuiUrl = values.GetValueOrDefault("webui-url", "http://localhost:8080");
		var webuiTimeout = ParseInt(values.GetValueOrDefault("webui-timeout", "180"), 180);
		var webuiPoll = ParseInt(values.GetValueOrDefault("webui-poll", "2"), 2);

		return new WrapperConfig
		{
			PackageRoot = packageRoot,
			BatLauncherPath = Path.Combine(packageRoot, "launch_z3r4h.bat"),
			LogFilePath = Path.Combine(packageRoot, "z3r4h_launcher.log"),
			LlamaServerExePath = llamaExe,
			LlamaModelPath = llamaModel,
			LlamaHost = llamaHost,
			LlamaPort = llamaPort,
			LlamaReadyTimeoutSeconds = llamaTimeout,
			LlamaReadyPollSeconds = llamaPoll,
			OpenWebUiStartCommand = webuiCmd,
			OpenWebUiUrl = webuiUrl,
			OpenWebUiReadyTimeoutSeconds = webuiTimeout,
			OpenWebUiReadyPollSeconds = webuiPoll,
			StartupMode = startupMode,
			FallbackMode = fallbackMode
		};
	}

	private static int ParseInt(string value, int fallback)
		=> int.TryParse(value, out var parsed) ? parsed : fallback;

	private static Dictionary<string, string> ParseArgs(string[] args)
	{
		var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (var arg in args)
		{
			if (!arg.StartsWith("--", StringComparison.Ordinal) || !arg.Contains('='))
			{
				continue;
			}

			var parts = arg[2..].Split('=', 2, StringSplitOptions.TrimEntries);
			if (parts.Length == 2)
			{
				dict[parts[0]] = parts[1];
			}
		}
		return dict;
	}
}
