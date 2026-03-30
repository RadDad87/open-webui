using System.Diagnostics;
using Z3R4H.Wrapper.Config;
using Z3R4H.Wrapper.Logging;

namespace Z3R4H.Wrapper.Fallback;

public sealed class BatFallback(WrapperLogger logger)
{
	public (bool Success, int ExitCode, string Message) InvokeFallback(WrapperConfig config, string reason)
	{
		logger.Error(
			WrapperErrorCategory.FallbackInvocation,
			$"Invoking BAT fallback because: {reason}. Target: {config.BatLauncherPath}");

		if (!File.Exists(config.BatLauncherPath))
		{
			return (false, -1, $"BAT fallback path not found: {config.BatLauncherPath}");
		}

		try
		{
			var psi = new ProcessStartInfo
			{
				FileName = "cmd.exe",
				Arguments = $"/c \"\"{config.BatLauncherPath}\"\"",
				WorkingDirectory = config.PackageRoot,
				UseShellExecute = false
			};

			using var process = Process.Start(psi);
			if (process is null)
			{
				return (false, -2, "Failed to start BAT fallback process.");
			}

			process.WaitForExit();
			logger.Info($"BAT fallback exited with code {process.ExitCode}");
			return (true, process.ExitCode, "BAT fallback completed.");
		}
		catch (Exception ex)
		{
			return (false, -3, $"Exception while invoking BAT fallback: {ex.Message}");
		}
	}
}
