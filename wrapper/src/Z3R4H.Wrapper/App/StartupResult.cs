using Z3R4H.Wrapper.Logging;

namespace Z3R4H.Wrapper.App;

public enum WrapperExitCode
{
	Success = 0,
	ConfigResolutionFailed = 10,
	ConfigValidationFailed = 11,
	UnsupportedMode = 12,
	LlamaLaunchFailed = 13,
	LlamaReadinessFailed = 14,
	WebUiLaunchFailed = 15,
	WebUiReadinessFailed = 16,
	BrowserLaunchFailed = 17,
	FallbackInvoked = 20,
	FallbackInvocationFailed = 21,
	UnhandledError = 99
}

public sealed record StartupResult(
	bool Success,
	int ExitCode,
	string Message,
	WrapperErrorCategory? ErrorCategory = null,
	string? Stage = null);
