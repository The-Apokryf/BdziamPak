namespace BdziamPak.Processing.Process.Progress;

public class ProcessStatus
{
    public event Action<ProcessStatus>? StatusChanged;
    private List<StepStatus> StepStatuses { get; } = new();
    
}