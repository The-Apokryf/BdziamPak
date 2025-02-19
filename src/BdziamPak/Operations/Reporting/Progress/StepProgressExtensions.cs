namespace BdziamPak.Operations.Reporting.Progress;

public static class StepProgressExtensions
{
    public static void Determinate(this StepProgress stepProgress, string name, int current, int? total = null)
    {
        stepProgress.Update<DeterminateProgressIndicator>(name, value =>
            {
                if (total.HasValue)
                    value.UpdateProgress(current, total.Value);
                value.UpdateProgress(current);
            })
            .Report();
    }

    public static void Increment(this StepProgress stepProgress, string name)
    {
        stepProgress.Update<DeterminateProgressIndicator>(name, value => value.UpdateProgress(value.Current + 1))
            .Report();
    }
    
    public static void Info(this StepProgress stepProgress, string name, string message)
    {
        stepProgress.Update<InfoProgressIndicator>(name, value =>
            {
                value.Update(message);
            })
            .Report();
    }
    
    public static void Status(this StepProgress stepProgress, string message)
    {
        stepProgress.Update<IndeterminateProgressIndicator>("Status", value =>
            {
                value.Update(message);
            })
            .Report();
    }

    
    public static void Indeterminate(this StepProgress stepProgress, string name, string message)
    {
        stepProgress.Update<IndeterminateProgressIndicator>(name, value => value.Update(message))
            .Report();
    }
}