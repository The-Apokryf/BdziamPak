using BdziamPak.Git;
using BdziamPak.Operations.Context;
using BdziamPak.Operations.Reporting.Progress;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.Operations.Steps.Validation;
using BdziamPak.Operations.Steps.Validation.BuiltIn;
using BdziamPak.PackageModel.Builtin;

namespace BdziamPak.Operations.Steps.BuiltIn;

/// <summary>
/// Represents a built-in step in the resolving process that clones a Git repository for the package.
/// </summary>
public class CloneRepository(GitService gitService) : BdziamPakOperationStep
{
    
    /// <inheritdoc />
    public override string StepName => "CloneRepository";

    private const string RepositoryMetadataKey ="Repository";
    
    public override void ValidateOperation(OperationValidationContext context)
    {
        context.AddCondition<HasMetadataCondition>(condition => condition.RequireMetadata(RepositoryMetadataKey));
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(IExecuteOperationContext context, IProgress<StepProgress> progress, CancellationToken cancellationToken = default)
    {
        StepState = StepState.Running;
        // Retrieve the repository metadata
        var repo = context.GetMetadata<BdziamPakRepositoryReference>(RepositoryMetadataKey)!;
        
        var cloneProgress = new Progress<CloneRepositoryProgress>();
        progress.Report(($"Cloning repository {repo.Url}", 0, new CloneRepositoryProgress(){Message = "Starting Process"}));

        cloneProgress.ProgressChanged += (sender, args) =>
        {
            progress.Report((args.Message, args.CloneProgress, args));
        };
        
        // Use GitService to clone the repository to the specified directory
        gitService.CloneRepo(context.ResolveDirectory, repo.Url, repo.CommitHash, cloneProgress, cancellationToken);
        
        StepState = StepState.Success;
        return Task.CompletedTask;
    }
}