using Bdziam.ExternalDependencyResolver;
using BdziamPak.Operations.Steps.BuiltIn;

namespace BdziamPak.Operations.Factory;

public class BuiltInOperationsFactory(ExternalDependencyResolver externalDependencyResolver) : IOperationFactory
{
    public BdziamPakOperation GetOperation(string operationName)
    {
        return operationName switch
        {
            "resolve" => new BdziamPakOperation(operationName, externalDependencyResolver)
                .AddStep<ResolveBdziamPakDependencies>()
                .AddStep<CloneRepository>()
                .AddStep<InstallNuGetPackage>()
        };
    }
}