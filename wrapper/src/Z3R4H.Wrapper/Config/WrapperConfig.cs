namespace Z3R4H.Wrapper.Config;

public sealed class WrapperConfig
{
	public required string PackageRoot { get; init; }
	public string PackageRootSource { get; init; } = "auto";
	public required string BatLauncherPath { get; init; }
	public required string LogFilePath { get; init; }

	public required string LlamaServerExePath { get; init; }
	public required string LlamaModelPath { get; init; }
	public string LlamaHost { get; init; } = "127.0.0.1";
	public int LlamaPort { get; init; } = 11434;
	public int LlamaReadyTimeoutSeconds { get; init; } = 120;
	public int LlamaReadyPollSeconds { get; init; } = 2;

	public string OpenWebUiStartCommand { get; init; } = "open-webui serve";
	public string OpenWebUiUrl { get; init; } = "http://localhost:8080";
	public int OpenWebUiReadyTimeoutSeconds { get; init; } = 180;
	public int OpenWebUiReadyPollSeconds { get; init; } = 2;

	public string StartupMode { get; init; } = "exe-first";
	public string FallbackMode { get; init; } = "auto"; // auto | always | never
}
