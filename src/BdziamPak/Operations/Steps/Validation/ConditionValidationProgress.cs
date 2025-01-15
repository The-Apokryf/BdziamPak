namespace BdziamPak.Operations.Steps.Validation;

public class ConditionValidationProgress(bool canExecute, string? message = null)
{
    public string? Message { get; } = message;
    public bool CanExecute { get; } = canExecute;
    
    public static implicit operator ConditionValidationProgress(bool result)
    {
        return new ConditionValidationProgress(result);
    }
}