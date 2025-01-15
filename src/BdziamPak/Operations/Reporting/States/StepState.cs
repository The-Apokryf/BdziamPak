namespace BdziamPak.Operations.Reporting.States;
/// <summary>
/// Describes state of the step in the process.
/// </summary>
public enum StepState
{
    /// <summary>
    /// The step is scheduled to be executed.
    /// </summary>
    Scheduled,

    /// <summary>
    /// The step is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// The step has been completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The step was skipped.
    /// </summary>
    Skipped,

    /// <summary>
    /// The step has failed.
    /// </summary>
    Failed
}