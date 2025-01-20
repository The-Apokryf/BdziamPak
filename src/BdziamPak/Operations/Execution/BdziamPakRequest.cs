namespace BdziamPak.Operations.Execution;

public class BdziamPakRequest(string bdziamPakId, string version)
{
    public string BdziamPakId { get; protected set; } = bdziamPakId;
    public string Version { get; protected set; } = version;

    public override string ToString()
    {
        return $"{BdziamPakId}@{Version}";
        ;
    }

    public static implicit operator BdziamPakRequest(string value)
    {
        var valueSplit = value.Split('@');
        if (valueSplit.Length != 2) throw new ArgumentException("Invalid BdziamPak request format.");
        return new BdziamPakRequest(valueSplit.First(), valueSplit.Last());
    }
}