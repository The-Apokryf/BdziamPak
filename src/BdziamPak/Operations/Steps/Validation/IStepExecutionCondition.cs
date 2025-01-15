using BdziamPak.Operations.Context;

namespace BdziamPak.Operations.Steps.Validation;

public interface IStepExecutionCondition
{
    ConditionValidationProgress Validate(BdziamPakOperationStep step, IValidationContext context);
}