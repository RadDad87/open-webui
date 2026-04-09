using Z3R4H.Wrapper.Logging;

namespace Z3R4H.Wrapper.Health;

public sealed class ReadinessProbe(WrapperLogger logger)
{
	private static readonly HttpClient Client = new();

	public Task<(bool Success, string Message)> WaitForLlamaReadyAsync(string endpoint, int timeoutSeconds, int pollSeconds, CancellationToken ct = default)
		=> WaitForEndpointReadyAsync(endpoint, timeoutSeconds, pollSeconds, "llama", ct);

	public Task<(bool Success, string Message)> WaitForWebUiReadyAsync(string endpoint, int timeoutSeconds, int pollSeconds, CancellationToken ct = default)
		=> WaitForEndpointReadyAsync(endpoint, timeoutSeconds, pollSeconds, "open-webui", ct);

	private async Task<(bool Success, string Message)> WaitForEndpointReadyAsync(string endpoint, int timeoutSeconds, int pollSeconds, string label, CancellationToken ct)
	{
		var timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));
		var poll = TimeSpan.FromSeconds(Math.Max(1, pollSeconds));
		var started = DateTime.UtcNow;

		while (DateTime.UtcNow - started < timeout)
		{
			ct.ThrowIfCancellationRequested();
			try
			{
				using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
				using var res = await Client.SendAsync(req, ct);
				if ((int)res.StatusCode >= 200 && (int)res.StatusCode < 500)
				{
					return (true, $"{label} readiness endpoint reachable: {endpoint}");
				}
			}
			catch
			{
				// Continue polling until timeout.
			}

			logger.Stage("health", $"Waiting for {label} readiness at {endpoint}...");
			await Task.Delay(poll, ct);
		}

		return (false, $"Timed out waiting for {label} readiness at {endpoint}");
	}
}
