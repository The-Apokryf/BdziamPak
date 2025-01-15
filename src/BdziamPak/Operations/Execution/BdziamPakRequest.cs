namespace BdziamPak.Operations.Execution;

public class BdziamPakRequest(string bdziamPakId, string version)
{
    public string BdziamPakId { get; protected set; }
    public string Version { get; protected set; }
    
    public static implicit operator string(BdziamPakRequest request)
    {
        return $"{request.BdziamPakId}@{request.Version}";
    }

    public override string ToString()
    {
        return this;
    }

    public static implicit operator BdziamPakRequest(string value)
    {
        var valueSplit = value.Split('@');
        if (valueSplit.Length != 2)
        {
            throw new ArgumentException("Invalid BdziamPak request format.");
        }
        return new BdziamPakRequest(valueSplit.First(),valueSplit.Last());
    }
    
    
}