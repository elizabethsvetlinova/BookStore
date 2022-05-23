namespace BookStore.Lib;

public interface IStore
{
    void Import(string catalogAsJson);
    int GetQuantity(string name);
    decimal Buy(params string[] basketByNames);
}