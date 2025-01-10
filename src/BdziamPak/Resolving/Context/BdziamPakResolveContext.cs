using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Resolving.ResolveSteps;
using BdziamPak.Structure;

namespace BdziamPak.Resolving;

/// <summary>
/// Represents the context for resolving a BdziamPak package.
/// </summary>
/// <param name="bdziamPakResolveProcess">The resolve process.</param>
/// <param name="directory">The directory where the package is located.</param>
public class BdziamPakResolveContext(BdziamPakResolveProcess bdziamPakResolveProcess, BdziamPakDirectory directory)
    : IExecutionResolveContext, ICheckResolveContext
{
    /// <summary>
    /// Gets the current resolve status.
    /// </summary>
    public ResolveStatus Status { get; protected set; } = new();

    /// <summary>
    /// Gets the current resolve state.
    /// </summary>
    public ResolveState State { get; protected set; }

    /// <summary>
    /// Gets the list of completed resolve steps.
    /// </summary>
    public List<BdziamPakResolveStep> CompletedResolveSteps { get; } = new();

    /// <summary>
    /// Checks if a specific resolve step was completed.
    /// </summary>
    /// <typeparam name="TStep">The type of the resolve step.</typeparam>
    /// <returns>true if the step was completed; otherwise, false.</returns>
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

    /// <summary>
    /// Checks if a file exists in the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the file.</param>
    /// <returns>true if the file exists; otherwise, false.</returns>
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

    /// <summary>
    /// Checks if a directory exists in the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory.</param>
    /// <returns>true if the directory exists; otherwise, false.</returns>
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

    /// <summary>
    /// Checks if the metadata contains a specific key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>true if the metadata contains the key; otherwise, false.</returns>
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

    /// <summary>
    /// Updates the resolve status with a message and optional percentage.
    /// </summary>
    /// <param name="message">The status message.</param>
    /// <param name="percent">The optional percentage.</param>
    public void UpdateStatus(string message, int? percent = null)
    {
        Status.AddStatus(Status.CurrentStep, message, percent);
    }

    /// <summary>
    /// Gets the resolve directory.
    /// </summary>
    public DirectoryInfo ResolveDirectory => new(Path.Combine(directory.PaksDirectory.FullName,
        $"{BdziamPakMetadata.BdziamPakId}@{BdziamPakMetadata.Version}"));

    /// <summary>
    /// Gets or sets the metadata for the BdziamPak package.
    /// </summary>
    public BdziamPakMetadata BdziamPakMetadata { get; set; }

    /// <summary>
    /// Marks the resolve process as failed with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    public void Fail(string message)
    {
        State = ResolveState.Failed;
        Status.AddStatus(Status.CurrentStep, message);
        bdziamPakResolveProcess.StepStopped(true, this);
    }

    /// <summary>
    /// Skips the current step with a message.
    /// </summary>
    /// <param name="message">The skip message.</param>
    public void Skip(string message)
    {
        Status.AddStatus(Status.CurrentStep, message);
        bdziamPakResolveProcess.StepStopped(false, this);
    }

    /// <summary>
    /// Gets a file from the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the file.</param>
    /// <returns>The file information.</returns>
    public FileInfo GetFile(string relativePath)
    {
        return new FileInfo(Path.Combine(ResolveDirectory.FullName, relativePath));
    }

    /// <summary>
    /// Gets a directory from the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory.</param>
    /// <returns>The directory information.</returns>
    public DirectoryInfo GetDirectory(string relativePath)
    {
        return new DirectoryInfo(Path.Combine(ResolveDirectory.FullName, relativePath));
    }

    /// <summary>
    /// Completes the current resolve step.
    /// </summary>
    public void Complete()
    {
        CompletedResolveSteps.Add(bdziamPakResolveProcess?.CurrentStep);
        Status.AddStatus(Status.CurrentStep, "Step completed");
        bdziamPakResolveProcess.StepResolveCompleted(this);
    }

    /// <summary>
    /// Gets the metadata value for the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value.</typeparam>
    /// <param name="key">The key of the metadata.</param>
    /// <returns>The metadata value if found; otherwise, the default value for the type.</returns>
    public T? GetMetadata<T>(string key)
    {
        var result = BdziamPakMetadata.GetMetadata<T>(key);
        if (result == null) Skip($"Step cannot abort, because the metadata {key} is required");
        return result;
    }

    /// <summary>
    /// Updates the resolve status with a message and optional percentage.
    /// </summary>
    /// <param name="step">The current step.</param>
    /// <param name="message">The status message.</param>
    /// <param name="percent">The optional percentage.</param>
    public void UpdateStatus(int step, string message, int? percent = null)
    {
        Status.AddStatus(Status.CurrentStep, message);
    }
}