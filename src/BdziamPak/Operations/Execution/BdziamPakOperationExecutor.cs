using Bdziam.ExternalDependencyResolver;
using BdziamPak.Directory;
using BdziamPak.Operations.Context;
using BdziamPak.Operations.Reporting.Progress;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.Operations.Steps.Validation;
using BdziamPak.PackageModel;

namespace BdziamPak.Operations.Execution;

/// <summary>
/// Executes BdziamPak operations.
/// </summary>
public class BdziamPakOperationExecutor(Sources.Sources sources, BdziamPakDirectory bdziamPakDirectory, ExternalDependencyResolver externalDependencyResolver)
{
    private CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    /// Executes the specified BdziamPak operation.
    /// </summary>
    /// <param name="requestedBdziamPak">The requested BdziamPak.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="progress">The progress reporter for the operation.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<OperationState> ExecuteOperation(BdziamPakRequest requestedBdziamPak, BdziamPakOperation operation, IProgress<BdziamPakOperationProgress> progress, CancellationToken ct = default)
    {
        ct.Register(() => _cancellationTokenSource.Cancel());
        var progressModel = new BdziamPakOperationProgress();
        progressModel.Message = $"Executing operation {operation.OperationName}";
        progressModel.InitSteps(operation.Steps);

        progress.Report(progressModel);

        progressModel.Message = $"Searching for requested bdziampak {requestedBdziamPak}";

        var bdziamPak = await GetBdziamPak(requestedBdziamPak, ct);

        if (bdziamPak == null)
        {
            progressModel.Message = "Requested bdziampak not found";
            progress.Report(progressModel);
            return OperationState.Failed;
        }

        var context = new BdziamPakOperationContext(operation)
        {
            BdziamPakMetadata = bdziamPak,
            RequestedVersion = requestedBdziamPak.Version,
            ResolveDirectory = new DirectoryInfo(Path.Combine(bdziamPakDirectory.PaksDirectory.FullName, requestedBdziamPak.ToString()))
        };

        foreach (var step in operation.Steps)
        {
            var stepProgressPercentage = 0;
            var stepProgress = new Progress<StepProgress>();
            progressModel.UpdateStep(step, ($"Executing step {step.StepName}", stepProgressPercentage));
            var stepCancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Token.Register(() => stepCancellationTokenSource.Cancel());

            if (ct.IsCancellationRequested)
            {
                progressModel.UpdateStep(step, ($"Executing step {step.StepName}", stepProgressPercentage));

                await stepCancellationTokenSource.CancelAsync();
                progressModel.Message = "Operation cancelled";
                progress.Report(progressModel);
                return OperationState.Failed;
            }

            stepProgress.ProgressChanged += (sender, args) =>
            {
                stepProgressPercentage = args.Percentage;
                progressModel.UpdateStep(step, args);
                progress.Report(progressModel);
                if (step.StepState == StepState.Failed)
                {
                    _cancellationTokenSource.Cancel();
                }

            };

            var validationProgress = new Progress<ConditionValidationResult>();
            validationProgress.ProgressChanged += (sender, conditionValidationProgress) =>
            {
                if (!conditionValidationProgress.CanExecute)
                {
                    progressModel.UpdateStep(step,
                        ($"Cannot execute step {step}, Details: {conditionValidationProgress.Message}",
                            stepProgressPercentage));
                    progress.Report(progressModel);
                    step.StepState = StepState.Skipped;
                }
            };

            progressModel.UpdateStep(step,
                ($"Validating step {step}",
                    stepProgressPercentage));
            progress.Report(progressModel);
            var validationContext = new OperationValidationContext(externalDependencyResolver, step, context);

            step.ValidateOperation(validationContext);
            var results = validationContext.GetResults();
            if (results.Any(x => !x.CanExecute))
            {
                step.StepState = StepState.Skipped;
                progressModel.UpdateStep(step,
                    ($"Skipping step {step}, reasons: {string.Join(",", results.Where(x => !string.IsNullOrEmpty(x.Message)).Select(x => x.Message))}",
                        stepProgressPercentage));
                continue;
            }

            step.StepState = StepState.Running;
            progressModel.Message = "Executing step " + step.StepName;
            progress.Report(progressModel);
            await step.ExecuteAsync(context, stepProgress, stepCancellationTokenSource.Token);


            return OperationState.Success;
        }

        return OperationState.Success;
    }

    /// <summary>
    /// Gets the BdziamPak metadata for the specified request.
    /// </summary>
    /// <param name="request">The request for the BdziamPak.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the BdziamPak metadata if found; otherwise, null.</returns>
    public async Task<BdziamPakMetadata?> GetBdziamPak(BdziamPakRequest request, CancellationToken cancellationToken = default)
    {
        var foundSources = (await sources.ListSourcesAsync(cancellationToken)).SelectMany(x => x.Paks);
        var foundPak = foundSources.FirstOrDefault(x => x.BdziamPakId == request.BdziamPakId && x.VersionExists(request.Version));
        return foundPak;
    }
}