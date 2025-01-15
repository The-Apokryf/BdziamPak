namespace BdziamPak.Git;

public class CloneRepositoryProgress
{
    public string? Message { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public string Path { get; set; }
}