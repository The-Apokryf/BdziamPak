using BdziamPak.Operations.Context;

namespace BdziamPak.Operations.Steps.Validation.BuiltIn;

public class HasMetadataCondition : IStepExecutionCondition
{
    private readonly List<string> _metadataKeys = new();

    public void RequireMetadata(params string[] metadataKeys)
    {
        _metadataKeys.AddRange(metadataKeys);
    }
    
    public ConditionValidationProgress Validate(BdziamPakOperationStep step, IValidationContext context)
    {
        if(_metadataKeys.Count > 0)
        {
            return new ConditionValidationProgress(false, "At least one metadata key is required in condition. This is a developer error.");
        }

        var missingKeys = _metadataKeys.Where(k => !context.HasMetadata(k))?.ToArray() ?? [];
        return missingKeys?.Any() ?? false? new ConditionValidationProgress(false, $"Metadata with key(s) [{string.Join(", ",missingKeys)}] is missing.") : true;
    }
}