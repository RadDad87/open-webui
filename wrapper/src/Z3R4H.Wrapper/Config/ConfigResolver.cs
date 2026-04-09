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
			PackageRootSource = packageRootSource,
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

	private static string ResolvePackageRootFromExe(string exeDir)
	{
		var current = new DirectoryInfo(Path.GetFullPath(exeDir));

		if (LooksLikePackageRoot(current.FullName))
		{
			return current.FullName;
		}

		// Published RC layout is expected as <package-root>\\wrapper\\Z3R4H.Wrapper.exe
		if (string.Equals(current.Name, "wrapper", StringComparison.OrdinalIgnoreCase)
			&& current.Parent is not null
			&& LooksLikePackageRoot(current.Parent.FullName))
		{
			return current.Parent.FullName;
		}

		// Defensive upward search for launch marker if layout differs.
		var probe = current;
		for (var i = 0; i < 6 && probe is not null; i++)
		{
			if (LooksLikePackageRoot(probe.FullName))
			{
				return probe.FullName;
			}
			probe = probe.Parent;
		}

		// Last-resort: stay anchored to executable directory rather than filesystem root.
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
