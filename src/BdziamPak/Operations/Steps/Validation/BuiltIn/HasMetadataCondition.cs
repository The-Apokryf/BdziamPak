using BdziamPak.Operations.Context;

namespace BdziamPak.Operations.Steps.Validation.BuiltIn;

public class HasMetadataCondition : IStepExecutionCondition
{
    private readonly List<string> _metadataKeys = new();

    public ConditionValidationResult Validate(BdziamPakOperationStep step, IValidationContext context)
    {
        if (_metadataKeys.Count == 0)
            return new ConditionValidationResult(false,
                "At least one metadata key is required in condition. This is a developer error.");

        var missingKeys = _metadataKeys.Where(k => !context.HasMetadata(k))?.ToArray() ?? [];
        return missingKeys?.Any() ?? false
            ? new ConditionValidationResult(false,
                $"Metadata with key(s) [{string.Join(", ", missingKeys)}] is missing.")
            : new ConditionValidationResult(true);
    }

    public void RequireMetadata(params string[] metadataKeys)
    {
        _metadataKeys.AddRange(metadataKeys);
    }
}