namespace BookStore.Lib;

public class NotEnoughInventoryException : Exception
{
    internal record NameQuantity(string Name, int Quantity) : INameQuantity;

    public NotEnoughInventoryException(IEnumerable<INameQuantity> missing)
    {
        Missing = missing;
    }

    public IEnumerable<INameQuantity> Missing { get; }
}
