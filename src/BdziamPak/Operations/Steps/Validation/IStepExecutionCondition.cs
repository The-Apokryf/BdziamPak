using BdziamPak.Operations.Context;

namespace BdziamPak.Operations.Steps.Validation;

public interface IStepExecutionCondition
{
    ConditionValidationResult Validate(BdziamPakOperationStep step, IValidationContext context);
}