using BdziamPak.Operations.Reporting.Progress;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.Operations.Steps;

namespace BdziamPak.Operations.Execution;

public class BdziamPakOperationProgress
{
    public List<BdziamPakStepProgress> Steps { get; set; } = new();
    public int Progress => Steps.Count(x => x.State is StepState.Success or StepState.Skipped) / Steps.Count;
    public string Message { get; set; }

    public void InitSteps(IEnumerable<BdziamPakOperationStep> steps)
    {
        foreach (var step in steps)
        {
            Steps.Add(new BdziamPakStepProgress
            {
                Name = step.StepName,
                State = step.StepState
            });
        }
    }
    
    public void UpdateStep(BdziamPakOperationStep step, StepProgress progress)
    {
        var foundStep = Steps.FirstOrDefault(x => x.Name == step.StepName);
        if (foundStep == null)
        {
            Steps.Add(new BdziamPakStepProgress
            {
                Name = step.StepName,
                State = step.StepState,
                Message = progress.Message,
                Percentage = progress.Percentage
            });
            return;
        }
        
        foundStep.State = step.StepState;
        foundStep.Message = progress.Message;
        foundStep.Percentage = progress.Percentage;
    }
}

public class BdziamPakStepProgress
{
    public string Name { get; set; }
    public string Message { get; set; }
    public int Percentage { get; set; }
    public StepState State { get; set; }
}