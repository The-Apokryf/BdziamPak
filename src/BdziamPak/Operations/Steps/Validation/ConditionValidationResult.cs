namespace BdziamPak.Operations.Steps.Validation;

public class ConditionValidationResult(bool canExecute, string? message = null)
{
    public string? Message { get; } = message;
    public bool CanExecute { get; } = canExecute;
    
    public static explicit operator ConditionValidationResult(bool result)
    {
        return new ConditionValidationResult(result);
    }
    
    public static implicit operator bool(ConditionValidationResult result)
    {
        return result.CanExecute;
    }
}