namespace Company.NameProject.Application.Common.Interfaces
{
    /// <summary>
    /// Marker interface. Commands/Queries that implement this will be wrapped in a DB transaction by TransactionBehavior.
    /// </summary>
    public interface IRequiresTransaction
    {
    }
}
