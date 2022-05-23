namespace BookStore.Lib;

public class InvalidCatalogException : Exception
{
    public InvalidCatalogException(IList<string> validationErrors, string message = "Invalid json catalog")
        : base(message)
    {
        ValidationErrors = validationErrors;
    }

    public IList<string> ValidationErrors { get; }

}
