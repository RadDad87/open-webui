using Z3R4H.Wrapper.App;
using Z3R4H.Wrapper.Config;
using Z3R4H.Wrapper.Fallback;
using Z3R4H.Wrapper.Health;
using Z3R4H.Wrapper.Logging;
using Z3R4H.Wrapper.Orchestration;

var logger = new WrapperLogger();
var resolver = new ConfigResolver();
var probe = new ReadinessProbe(logger);
var orchestrator = new RuntimeOrchestrator(logger, probe);
var fallback = new BatFallback(logger);

var app = new WrapperApp(logger, resolver, orchestrator, fallback);
var result = await app.RunAsync(args);

if (!result.Success)
{
	logger.Error(result.ErrorCategory ?? WrapperErrorCategory.StartupFailure, $"Stage={result.Stage}; ExitCode={result.ExitCode}; {result.Message}");
}
else
{
	logger.Info($"Stage={result.Stage}; ExitCode={result.ExitCode}; {result.Message}");
}

return result.ExitCode;
