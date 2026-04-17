namespace Z3R4H.Wrapper.UI;

public sealed class StartupScreen
{
	private readonly object _sync = new();

	public void ShowInitializing()
	{
		lock (_sync)
		{
			Console.Clear();
			Console.WriteLine("========================================");
			Console.WriteLine("      Initializing Z3R4H System...");
			Console.WriteLine("========================================");
			Console.WriteLine();
		}
	}

	public void StepStart(string step)
	{
		lock (_sync)
		{
			Console.WriteLine($"[ ... ] {step}");
		}
	}

	public void StepSuccess(string step)
	{
		lock (_sync)
		{
			Console.WriteLine($"[  OK ] {step}");
		}
	}

	public void StepWarning(string step, string detail)
	{
		lock (_sync)
		{
			Console.WriteLine($"[WARN ] {step} - {detail}");
		}
	}

	public void StepFailure(string step, string detail)
	{
		lock (_sync)
		{
			Console.WriteLine($"[FAIL ] {step}");
			Console.WriteLine($"         {detail}");
		}
	}

	public void Complete(bool basicMode)
	{
		lock (_sync)
		{
			Console.WriteLine();
			Console.WriteLine(basicMode
				? "Z3R4H launched in BASIC mode (maps without routing)."
				: "Z3R4H fully initialized.");
		}
	}
}
