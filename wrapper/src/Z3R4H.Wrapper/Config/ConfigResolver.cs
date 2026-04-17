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

		var runtimeConfigPath = ResolveAgainstRoot(packageRoot, values.GetValueOrDefault("runtime-config", "runtime/runtime_config.env"));
		var runtimeConfigValues = File.Exists(runtimeConfigPath)
			? ParseKeyValueFile(runtimeConfigPath)
			: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		string GetValue(string key, string fallback)
		{
			if (values.TryGetValue(key, out var argValue) && !string.IsNullOrWhiteSpace(argValue))
			{
				return argValue;
			}
			if (runtimeConfigValues.TryGetValue(key.Replace('-', '_').ToUpperInvariant(), out var runtimeValue) && !string.IsNullOrWhiteSpace(runtimeValue))
			{
				return runtimeValue;
			}
			return fallback;
		}

		string ResolvePathValue(string key, string fallback)
		{
			var value = GetValue(key, fallback);
			return ResolveAgainstRoot(packageRoot, value);
		}

		var startupMode = GetValue("startup-mode", "exe-first");
		var fallbackMode = GetValue("fallback", "auto");
		var aiMode = GetValue("ai-mode", "auto");

		var runtimeBackendDir = ResolvePathValue("runtime-backend-dir", "backend");
		var runtimeFrontendDir = ResolvePathValue("runtime-frontend-dir", "build");
		var runtimeModelsDir = ResolvePathValue("runtime-models-dir", "models");
		var runtimeRoutingDir = ResolvePathValue("runtime-routing-dir", "runtime/routing");
		var runtimeDataDir = ResolvePathValue("runtime-data-dir", "data/geo");
		var runtimeDemDir = ResolvePathValue("runtime-dem-dir", "data/geo/dem");
		var runtimeLogsDir = ResolvePathValue("runtime-logs-dir", "logs");
		var runtimeCacheDir = ResolvePathValue("runtime-cache-dir", "cache");

		Directory.CreateDirectory(runtimeLogsDir);
		Directory.CreateDirectory(runtimeCacheDir);

		var llamaExe = ResolvePathValue("llama-exe", "runtime/llama.cpp/llama-server.exe");
		var llamaModel = ResolvePathValue("llama-model", "models/model.gguf");
		var llamaHost = GetValue("llama-host", "127.0.0.1");
		var llamaPort = ParseInt(GetValue("llama-port", "11434"), 11434);

		var ollamaExe = ResolvePathValue("ollama-exe", "runtime/ollama/ollama.exe");
		var ollamaHost = GetValue("ollama-host", "127.0.0.1");
		var ollamaPort = ParseInt(GetValue("ollama-port", "11434"), 11434);

		var backendCmd = ResolveCommand(GetValue("backend-cmd", "open-webui serve"), packageRoot);
		var backendUrl = GetValue("backend-url", "http://localhost:8080").TrimEnd('/');
		var backendHealth = GetValue("backend-health", $"{backendUrl}/health");

		var valhallaCmd = ResolveCommand(GetValue("valhalla-cmd", string.Empty), packageRoot);
		var valhallaHealth = GetValue("valhalla-health", "http://127.0.0.1:8002/status");
		var geoHealth = GetValue("geo-health", $"{backendUrl}/api/geo/health");

		var mapsPath = GetValue("maps-path", "/maps");
		if (!mapsPath.StartsWith('/'))
		{
			mapsPath = "/" + mapsPath;
		}

		return new WrapperConfig
		{
			PackageRoot = packageRoot,
			PackageRootSource = packageRootSource,
			BatLauncherPath = Path.Combine(packageRoot, "launch_z3r4h.bat"),
			RuntimeConfigPath = runtimeConfigPath,
			LogFilePath = Path.Combine(runtimeLogsDir, "z3r4h_wrapper.log"),
			RuntimeBackendDir = runtimeBackendDir,
			RuntimeFrontendDir = runtimeFrontendDir,
			RuntimeModelsDir = runtimeModelsDir,
			RuntimeRoutingDir = runtimeRoutingDir,
			RuntimeDataDir = runtimeDataDir,
			RuntimeDemDir = runtimeDemDir,
			RuntimeLogsDir = runtimeLogsDir,
			RuntimeCacheDir = runtimeCacheDir,
			AiMode = aiMode,
			LlamaServerExePath = llamaExe,
			LlamaModelPath = llamaModel,
			LlamaHost = llamaHost,
			LlamaPort = llamaPort,
			LlamaReadyTimeoutSeconds = ParseInt(GetValue("llama-timeout", "120"), 120),
			LlamaReadyPollSeconds = ParseInt(GetValue("llama-poll", "2"), 2),
			OllamaExePath = ollamaExe,
			OllamaHost = ollamaHost,
			OllamaPort = ollamaPort,
			OllamaReadyTimeoutSeconds = ParseInt(GetValue("ollama-timeout", "120"), 120),
			OllamaReadyPollSeconds = ParseInt(GetValue("ollama-poll", "2"), 2),
			BackendStartCommand = backendCmd,
			BackendUrl = backendUrl,
			BackendHealthUrl = backendHealth,
			BackendReadyTimeoutSeconds = ParseInt(GetValue("backend-timeout", "180"), 180),
			BackendReadyPollSeconds = ParseInt(GetValue("backend-poll", "2"), 2),
			ValhallaStartCommand = valhallaCmd,
			ValhallaHealthUrl = valhallaHealth,
			ValhallaReadyTimeoutSeconds = ParseInt(GetValue("valhalla-timeout", "120"), 120),
			ValhallaReadyPollSeconds = ParseInt(GetValue("valhalla-poll", "2"), 2),
			GeoHealthUrl = geoHealth,
			GeoHealthTimeoutSeconds = ParseInt(GetValue("geo-timeout", "120"), 120),
			GeoHealthPollSeconds = ParseInt(GetValue("geo-poll", "2"), 2),
			BrowserStartUrl = GetValue("browser-url", $"{backendUrl}{mapsPath}"),
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

	private static string ResolveAgainstRoot(string root, string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return value;
		}
		if (Path.IsPathRooted(value))
		{
			return Path.GetFullPath(value);
		}
		return Path.GetFullPath(Path.Combine(root, value));
	}

	private static string ResolveCommand(string command, string packageRoot)
	{
		if (string.IsNullOrWhiteSpace(command))
		{
			return string.Empty;
		}

		if (command.Contains(' ') || command.Contains('"'))
		{
			return command;
		}

		var candidate = ResolveAgainstRoot(packageRoot, command);
		if (File.Exists(candidate))
		{
			return $"\"{candidate}\"";
		}

		return command;
	}

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

	private static Dictionary<string, string> ParseKeyValueFile(string path)
	{
		var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (var raw in File.ReadAllLines(path))
		{
			var line = raw.Trim();
			if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
			{
				continue;
			}

			var idx = line.IndexOf('=');
			if (idx <= 0)
			{
				continue;
			}

			var key = line[..idx].Trim();
			var val = line[(idx + 1)..].Trim();
			dict[key] = val;
		}

		return dict;
	}
}
