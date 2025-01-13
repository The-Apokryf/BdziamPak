using BdziamPak.Git;
using BdziamPak.Packages.Packaging.Model;

namespace BdziamPak.Resolving.ResolveSteps.BuiltIn;

/// <summary>
/// Represents a built-in step in the resolving process that clones a Git repository for the package.
/// </summary>
public class CloneRepositoryStep(GitService gitService) : BdziamPakProcessStep
{
    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    public override string StepName => "CloneRepository";

    /// <summary>
    /// Determines whether this step can execute based on the provided context.
    /// </summary>
    /// <param name="context">The context to check for execution.</param>
    /// <returns>
    /// True if the context contains the "Repository" metadata; otherwise, false.
    /// </returns>
    public override bool CanExecute(ICheckProcessingContext context)
    {
        return context.HasMetadata("Repository");
    }

    /// <summary>
    /// Executes the step by cloning the specified Git repository.
    /// </summary>
    /// <param name="context">The execution context containing the metadata and resolve directory.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ExecuteAsync(IExecutionProcessingContext context)
    {
        // Update the status to indicate the cloning process has started
        context.UpdateStatus("Cloning repository...");
        
        // Retrieve the repository metadata
        var repo = context.GetMetadata<BdziamPakRepositoryReference>("Repository")!;
        
        // Use GitService to clone the repository to the specified directory
        gitService.CloneRepo(context.ResolveDirectory, repo.Url, repo.CommitHash);
        
        // Mark the step as complete
        context.Complete();
    }
}