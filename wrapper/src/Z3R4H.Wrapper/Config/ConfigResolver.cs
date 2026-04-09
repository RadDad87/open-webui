using System.Reflection;

namespace Z3R4H.Wrapper.Config;

public sealed class ConfigResolver
{
	public WrapperConfig Resolve(string[] args)
	{
		var values = ParseArgs(args);
		var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
		var hasPackageRootOverride = values.TryGetValue("package-root", out var configuredRoot);
		var packageRoot = hasPackageRootOverride
			? Path.GetFullPath(configuredRoot!)
			: ResolvePackageRootFromExe(exeDir);
		var packageRootSource = hasPackageRootOverride ? "arg" : "auto";

		var startupMode = values.GetValueOrDefault("startup-mode", "exe-first");
		var fallbackMode = values.GetValueOrDefault("fallback", "auto");
		var aiMode = values.GetValueOrDefault("ai-mode", "auto");

		var llamaExe = values.GetValueOrDefault(
			"llama-exe",
			Path.Combine(packageRoot, "runtime", "llama.cpp", "llama-server.exe"));
		var llamaModel = values.GetValueOrDefault(
			"llama-model",
			Path.Combine(packageRoot, "models", "model.gguf"));
		var llamaHost = values.GetValueOrDefault("llama-host", "127.0.0.1");
		var llamaPort = ParseInt(values.GetValueOrDefault("llama-port", "11434"), 11434);

		var ollamaExe = values.GetValueOrDefault(
			"ollama-exe",
			Path.Combine(packageRoot, "runtime", "ollama", "ollama.exe"));
		var ollamaHost = values.GetValueOrDefault("ollama-host", "127.0.0.1");
		var ollamaPort = ParseInt(values.GetValueOrDefault("ollama-port", "11434"), 11434);

		var backendCmd = values.GetValueOrDefault("backend-cmd", "open-webui serve");
		var backendUrl = values.GetValueOrDefault("backend-url", "http://localhost:8080").TrimEnd('/');
		var backendHealth = values.GetValueOrDefault("backend-health", $"{backendUrl}/health");

		var valhallaCmd = values.GetValueOrDefault("valhalla-cmd", string.Empty);
		var valhallaHealth = values.GetValueOrDefault("valhalla-health", "http://127.0.0.1:8002/status");
		var geoHealth = values.GetValueOrDefault("geo-health", $"{backendUrl}/api/geo/health");

		var mapsPath = values.GetValueOrDefault("maps-path", "/maps");
		if (!mapsPath.StartsWith('/'))
		{
			mapsPath = "/" + mapsPath;
		}

		return new WrapperConfig
		{
			PackageRoot = packageRoot,
			PackageRootSource = packageRootSource,
			BatLauncherPath = Path.Combine(packageRoot, "launch_z3r4h.bat"),
			LogFilePath = Path.Combine(packageRoot, "z3r4h_launcher.log"),
			AiMode = aiMode,
			LlamaServerExePath = llamaExe,
			LlamaModelPath = llamaModel,
			LlamaHost = llamaHost,
			LlamaPort = llamaPort,
			LlamaReadyTimeoutSeconds = ParseInt(values.GetValueOrDefault("llama-timeout", "120"), 120),
			LlamaReadyPollSeconds = ParseInt(values.GetValueOrDefault("llama-poll", "2"), 2),
			OllamaExePath = ollamaExe,
			OllamaHost = ollamaHost,
			OllamaPort = ollamaPort,
			OllamaReadyTimeoutSeconds = ParseInt(values.GetValueOrDefault("ollama-timeout", "120"), 120),
			OllamaReadyPollSeconds = ParseInt(values.GetValueOrDefault("ollama-poll", "2"), 2),
			BackendStartCommand = backendCmd,
			BackendUrl = backendUrl,
			BackendHealthUrl = backendHealth,
			BackendReadyTimeoutSeconds = ParseInt(values.GetValueOrDefault("backend-timeout", "180"), 180),
			BackendReadyPollSeconds = ParseInt(values.GetValueOrDefault("backend-poll", "2"), 2),
			ValhallaStartCommand = valhallaCmd,
			ValhallaHealthUrl = valhallaHealth,
			ValhallaReadyTimeoutSeconds = ParseInt(values.GetValueOrDefault("valhalla-timeout", "120"), 120),
			ValhallaReadyPollSeconds = ParseInt(values.GetValueOrDefault("valhalla-poll", "2"), 2),
			GeoHealthUrl = geoHealth,
			GeoHealthTimeoutSeconds = ParseInt(values.GetValueOrDefault("geo-timeout", "120"), 120),
			GeoHealthPollSeconds = ParseInt(values.GetValueOrDefault("geo-poll", "2"), 2),
			BrowserStartUrl = values.GetValueOrDefault("browser-url", $"{backendUrl}{mapsPath}"),
			StartupMode = startupMode,
			FallbackMode = fallbackMode
		};
	}

	private static string ResolvePackageRootFromExe(string exeDir)
	{
		var current = new DirectoryInfo(Path.GetFullPath(exeDir));

		if (LooksLikePackageRoot(current.FullName))
		{
			return current.FullName;
		}

		if (string.Equals(current.Name, "wrapper", StringComparison.OrdinalIgnoreCase)
			&& current.Parent is not null
			&& LooksLikePackageRoot(current.Parent.FullName))
		{
			return current.Parent.FullName;
		}

		var probe = current;
		for (var i = 0; i < 6 && probe is not null; i++)
		{
			if (LooksLikePackageRoot(probe.FullName))
			{
				return probe.FullName;
			}
			probe = probe.Parent;
		}

		return current.FullName;
	}

	private static bool LooksLikePackageRoot(string directory)
	{
		return File.Exists(Path.Combine(directory, "launch_z3r4h.bat"));
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
