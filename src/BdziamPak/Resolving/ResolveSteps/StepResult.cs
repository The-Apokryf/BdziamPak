namespace BdziamPak.Resolving.ResolveSteps;

public class StepResult(bool success, string message)
{
    public bool Success => success;
    public string Message => message;
}