namespace Z3R4H.Wrapper.Config;

public sealed class WrapperConfig
{
	public required string PackageRoot { get; init; }
	public string PackageRootSource { get; init; } = "auto";
	public required string BatLauncherPath { get; init; }
	public required string LogFilePath { get; init; }

	public string AiMode { get; init; } = "auto"; // auto | llama | ollama
	public required string LlamaServerExePath { get; init; }
	public required string LlamaModelPath { get; init; }
	public string LlamaHost { get; init; } = "127.0.0.1";
	public int LlamaPort { get; init; } = 11434;
	public int LlamaReadyTimeoutSeconds { get; init; } = 120;
	public int LlamaReadyPollSeconds { get; init; } = 2;

	public required string OllamaExePath { get; init; }
	public string OllamaHost { get; init; } = "127.0.0.1";
	public int OllamaPort { get; init; } = 11434;
	public int OllamaReadyTimeoutSeconds { get; init; } = 120;
	public int OllamaReadyPollSeconds { get; init; } = 2;

	public string BackendStartCommand { get; init; } = "open-webui serve";
	public string BackendUrl { get; init; } = "http://localhost:8080";
	public string BackendHealthUrl { get; init; } = "http://localhost:8080/health";
	public int BackendReadyTimeoutSeconds { get; init; } = 180;
	public int BackendReadyPollSeconds { get; init; } = 2;

	public string ValhallaStartCommand { get; init; } = string.Empty;
	public string ValhallaHealthUrl { get; init; } = "http://127.0.0.1:8002/status";
	public int ValhallaReadyTimeoutSeconds { get; init; } = 120;
	public int ValhallaReadyPollSeconds { get; init; } = 2;

	public string GeoHealthUrl { get; init; } = "http://localhost:8080/api/geo/health";
	public int GeoHealthTimeoutSeconds { get; init; } = 120;
	public int GeoHealthPollSeconds { get; init; } = 2;

	public string BrowserStartUrl { get; init; } = "http://localhost:8080/maps";

	public string StartupMode { get; init; } = "exe-first";
	public string FallbackMode { get; init; } = "auto"; // auto | always | never
}
