using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Resolving.ResolveSteps;
using BdziamPak.Structure;

namespace BdziamPak.Resolving;

public class BdziamPakResolveContext(BdziamPakResolveProcess bdziamPakResolveProcess, BdziamPakDirectory directory)
    : IExecutionResolveContext, ICheckResolveContext
{
    public ResolveStatus Status { get; protected set; } = new();
    public ResolveState State { get; protected set; }
    public List<BdziamPakResolveStep> CompletedResolveSteps { get; } = new();

    public bool WasCompleted<TStep>() where TStep : BdziamPakResolveStep
    {
        var result = CompletedResolveSteps.FirstOrDefault(step => step.GetType() == typeof(TStep));
        if (result == null)
        {
            Skip($"Step cannot abort, because the step {result.StepName} was not completed");
            return false;
        }

        return true;
    }

    public bool FileExists(string relativePath)
    {
        var result = new FileInfo(Path.Combine(ResolveDirectory.FullName, relativePath)).Exists;
        if (!result)
        {
            Skip($"Step cannot continue, because the file {relativePath} was not found");
            return false;
        }

        return result;
    }

    public bool DirectoryExists(string relativePath)
    {
        var result = new DirectoryInfo(Path.Combine(ResolveDirectory.FullName, relativePath)).Exists;
        if (!result)
        {
            Skip($"Step cannot continue, because the directory {relativePath} was not found");
            return false;
        }

        return result;
    }

    public bool HasMetadata(string key)
    {
        var result = BdziamPakMetadata.HasMetadata(key);
        if (!result)
        {
            Skip($"Step cannot continue, because the metadata {key} is required");
            return false;
        }

        return result;
    }

    public void UpdateStatus(string message, int? percent = null)
    {
        Status.AddStatus(Status.CurrentStep, message, percent);
    }

    public DirectoryInfo ResolveDirectory => new(Path.Combine(directory.PaksDirectory.FullName,
        $"{BdziamPakMetadata.BdziamPakId}@{BdziamPakMetadata.Version}"));

    public BdziamPakMetadata BdziamPakMetadata { get; set; }

    public void Fail(string message)
    {
        State = ResolveState.Failed;
        Status.AddStatus(Status.CurrentStep, message);
        bdziamPakResolveProcess.StepStopped(true, this);
    }

    public void Skip(string message)
    {
        Status.AddStatus(Status.CurrentStep, message);
        bdziamPakResolveProcess.StepStopped(false, this);
    }

    public FileInfo GetFile(string relativePath)
    {
        return new FileInfo(Path.Combine(ResolveDirectory.FullName, relativePath));
    }

    public DirectoryInfo GetDirectory(string relativePath)
    {
        return new DirectoryInfo(Path.Combine(ResolveDirectory.FullName, relativePath));
    }

    public void Complete()
    {
        CompletedResolveSteps.Add(bdziamPakResolveProcess?.CurrentStep);
        Status.AddStatus(Status.CurrentStep, "Step completed");
        bdziamPakResolveProcess.StepResolveCompleted(this);
    }

    public T? GetMetadata<T>(string key)
    {
        var result = BdziamPakMetadata.GetMetadata<T>(key);
        if (result == null) Skip($"Step cannot abort, because the metadata {key} is required");
        return result;
    }

    public void UpdateStatus(int step, string message, int? percent = null)
    {
        Status.AddStatus(Status.CurrentStep, message);
    }
}