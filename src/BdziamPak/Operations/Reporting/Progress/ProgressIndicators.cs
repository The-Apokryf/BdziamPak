namespace BdziamPak.Operations.Reporting.Progress;

public class ProgressIndicators(StepProgress progress)
{
    private readonly List<ProgressIndicator> _progress = new();
    public IReadOnlyList<ProgressIndicator> Progress => _progress;

    public int PercentageCompleted => _progress.Count(p => p.IsFinished) / Math.Max(1, _progress.Count) * 100;

    public void UpdateProgress<T>(string name, Action<T>? updateFunc) where T : ProgressIndicator
    {
        var progressItem = _progress.FirstOrDefault(p => p.Name == name);
        if (progressItem == null)
        {
            progressItem = Activator.CreateInstance(typeof(T), name) as T ??
                           throw new InvalidOperationException("Failed to create a new instance of the type.");
            _progress.Add(progressItem);
        }

        if (progressItem is T value && updateFunc != null)
        {
            updateFunc.Invoke(value);
            var index = _progress.IndexOf(progressItem);
        }

        progress.Report();
    }

    public void Finish(string name, bool isError = false)
    {
        var progressItem = _progress.FirstOrDefault(p => p.Name == name);
        if (progressItem == null)
        {
            return;
        }
        
        progressItem.Finish(isError);
    }
}