namespace BdziamPak.Operations.Reporting.Progress;

public class StepProgress
{
    public string Message { get; set; }
    public int Percentage { get; set; }
    
    public object? ProgressModel { get; set; }

    public static implicit operator StepProgress((string message, int percentage, object? progressModel) progress)
    {
        return new StepProgress()
        {
            Message = progress.message,
            Percentage = progress.percentage,
            ProgressModel = progress
        };
    }
    
    public static implicit operator StepProgress((string message, int percentage) progress)
    {
        return new StepProgress()
        {
            Message = progress.message,
            Percentage = progress.percentage
        };
    }
}