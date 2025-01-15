using Bdziam.ExternalDependencyResolver;
using BdziamPak.Operations.Context;

namespace BdziamPak.Operations.Steps.Validation;

public class OperationValidationContext(ExternalDependencyResolver externalDependencyResolver, BdziamPakOperationStep step, BdziamPakOperationContext context)
{
    private readonly List<IStepExecutionCondition> _conditions = new();
    public OperationValidationContext AddCondition<TCondition>(Action<TCondition>? configureCondition) where TCondition : IStepExecutionCondition
    {
        var conditionInstance = externalDependencyResolver.Resolve<TCondition>();
        configureCondition?.Invoke(conditionInstance);
        _conditions.Add(conditionInstance);
        return this;
    }
    
    public bool CanExecute(IProgress<ConditionValidationProgress> progress)
    {
        foreach (var condition in _conditions)
        {
            var result = condition.Validate(step, context);
            progress.Report(result);
            if (!result.CanExecute)
            {
                return false;
            }
        }

        return true;
    }

}