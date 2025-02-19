using Bdziam.ExternalDependencyResolver;
using BdziamPak.Directory;
using BdziamPak.Operations.Context;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.Operations.Steps.Validation;
using BdziamPak.PackageModel;

namespace BdziamPak.Operations.Execution;

/// <summary>
///     Executes BdziamPak operations.
/// </summary>
public class BdziamPakOperationExecutor(
    Sources.Sources sources,
    BdziamPakDirectory bdziamPakDirectory,
    ExternalDependencyResolver externalDependencyResolver)
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    ///     Executes the specified BdziamPak operation.
    /// </summary>
    /// <param name="requestedBdziamPak">The requested BdziamPak.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="progress">The progress reporter for the operation.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteOperation(BdziamPakRequest requestedBdziamPak, BdziamPakOperation operation,
        IProgress<BdziamPakOperationProgress> progress, CancellationToken ct = default)
    {
        ct.Register(() => _cancellationTokenSource.Cancel());
        var operationProgress = new BdziamPakOperationProgress(progress, operation);

        operationProgress.Update(
            $"Finding requested BdziamPak {requestedBdziamPak.BdziamPakId} with version {requestedBdziamPak.Version}",
            OperationState.Started);


        var bdziamPak = await GetBdziamPak(requestedBdziamPak, ct);

        if (bdziamPak == null)
        {
            operationProgress.Update(
                $"There's no {requestedBdziamPak.BdziamPakId} with version {requestedBdziamPak.Version}",
                OperationState.Failed);
            return;
        }

        var context = new BdziamPakOperationContext(operation)
        {
            BdziamPakMetadata = bdziamPak,
            RequestedVersion = requestedBdziamPak.Version,
            ResolveDirectory = new DirectoryInfo(Path.Combine(bdziamPakDirectory.PaksDirectory.FullName,
                requestedBdziamPak.ToString()))
        };

        foreach (var step in operation.Steps)
        {
            var stepProgress = operationProgress.GetStepProgress(step) ??
                               throw new InvalidOperationException("Step progress not found");

            stepProgress.UpdateAndReport("Initializing...");

            var stepCancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Token.Register(() => stepCancellationTokenSource.Cancel());

            if (ct.IsCancellationRequested)
            {
                stepProgress.UpdateAndReport("Operation cancelled", StepState.Failed);
                await stepCancellationTokenSource.CancelAsync();
            }

            operationProgress.Steps[stepProgress].ProgressChanged += (sender, args) =>
            {
                if (step.StepState == StepState.Failed) _cancellationTokenSource.Cancel();
            };
            operationProgress.Update($"Validating step {step.StepName}");
            stepProgress.UpdateAndReport("Validating...", StepState.Scheduled);
            var validationContext = new OperationValidationContext(externalDependencyResolver, step, context);

            step.ValidateOperation(validationContext);
            var results = validationContext.GetResults();
            
            if (results.Any(x => !x.CanExecute))
            {
                step.StepState = StepState.Skipped;
                operationProgress.Update($"Skipping step {step}, reasons: {string.Join(",", results.Where(x => !string.IsNullOrEmpty(x.Message)).Select(x => x.Message))}");
                stepProgress.UpdateAndReport(
                    string.Join(",", results.Where(x => !string.IsNullOrEmpty(x.Message)).Select(x => x.Message)),
                    StepState.Skipped);
                continue;
            }

            stepProgress.UpdateAndReport("Validation Complete, Running Step", StepState.Running);
            await step.ExecuteAsync(context, stepProgress, stepCancellationTokenSource.Token);
            stepProgress.UpdateAndReport("Step Complete", step.StepState);
        }

        operationProgress.Update("Operation Complete", OperationState.Success);
    }

    /// <summary>
    ///     Gets the BdziamPak metadata for the specified request.
    /// </summary>
    /// <param name="request">The request for the BdziamPak.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the BdziamPak metadata if found;
    ///     otherwise, null.
    /// </returns>
    public async Task<BdziamPakMetadata?> GetBdziamPak(BdziamPakRequest request,
        CancellationToken cancellationToken = default)
    {
        var foundSources = (await sources.ListSourcesAsync(cancellationToken)).SelectMany(x => x.Paks);
        var foundPak =
            foundSources.FirstOrDefault(x => x.BdziamPakId == request.BdziamPakId && x.VersionExists(request.Version));
        return foundPak;
    }
}