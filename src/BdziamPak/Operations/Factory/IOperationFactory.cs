namespace BdziamPak.Operations.Factory;

public interface IOperationFactory
{
   public BdziamPakOperation GetOperation(string operationName);
}