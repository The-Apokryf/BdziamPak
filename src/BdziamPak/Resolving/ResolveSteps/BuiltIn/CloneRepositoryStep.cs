using BdziamPak.Git;
using BdziamPak.Packages.Packaging.Model;

namespace BdziamPak.Resolving.ResolveSteps.BuiltIn;

public class CloneRepositoryStep(GitService gitService) : BdziamPakResolveStep
{
    public override string StepName => "CloneRepository";
    public override string StepDescription => "Clones the repository for the package.";

    public override bool CanExecute(ICheckResolveContext context)
    {
        return context.HasMetadata("Repository");
    }

    public override async Task ExecuteAsync(IExecutionResolveContext context)
    {
        context.UpdateStatus("Cloning repository...");
        var repo = context.GetMetadata<BdziamPakRepositoryReference>("Repository")!;
        gitService.CloneRepo(context.ResolveDirectory, repo.Url, repo.CommitHash);
        context.Complete();
    }
}