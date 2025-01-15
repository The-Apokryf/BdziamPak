namespace BdziamPak.Git;

public class CloneRepositoryProgress
{
    public string? Message { get; set; }

    public int FetchProgress { get; set; } = 0;
    
    public int CheckoutProgress { get; set; } = 0;

    public int CloneProgress => (FetchProgress + CheckoutProgress / 200);
    public string Path { get; set; }
}