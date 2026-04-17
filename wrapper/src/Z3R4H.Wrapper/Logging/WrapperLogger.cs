namespace Z3R4H.Wrapper.Logging;

public enum WrapperErrorCategory
{
	ConfigError,
	PathError,
	ModeError,
	PortConflict,
	ProbeFailure,
	Timeout,
	StartupFailure,
	FallbackInvocation,
	UnhandledError
}

public sealed class WrapperLogger
{
	public void Stage(string stage, string message)
	{
		Console.WriteLine($"[{Timestamp()}] [stage:{stage}] {message}");
	}

	public void Info(string message)
	{
		Console.WriteLine($"[{Timestamp()}] [info] {message}");
	}

	public void Error(WrapperErrorCategory category, string message)
	{
		Console.Error.WriteLine($"[{Timestamp()}] [error:{category}] {message}");
	}

	private static string Timestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}
