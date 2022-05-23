namespace BookStore.Lib.Memory;

internal partial class SimpleMemoryStore : IStore
{
    internal record Book(string Name, string Category, decimal Price, int Quantity);

    internal Dictionary<string, decimal> CategoryDiscounts { get; } = new();
    private Dictionary<string, Book> books { get; } = new();

    public decimal Buy(params string[] bookNamesFromBasket)
    {
        var bookNameAndQuantity = bookNamesFromBasket.GroupBy(x => x).ToDictionary(x => x.Key, g => g.Count());

        if (!ValidateBooksQuantities(bookNameAndQuantity, out var validationErrors))
        {
            throw new NotEnoughInventoryException(validationErrors);
        }

        var books = GetBooksFromStock(bookNameAndQuantity);

        return CalculatePrice(books);
    }

    public int GetQuantity(string name)
    {
        return books.ContainsKey(name) ? books[name].Quantity : 0;
    }

    public void Import(string catalogAsJson)
    {
        JObject catalog = JObject.Parse(catalogAsJson);

        IList<string> errorMessages;
        var schema = JSchema.Parse(DefaultMemoryStoreConstants.Schema);
        bool valid = catalog.IsValid(schema, out errorMessages);

        if (!valid)
        {
            throw new InvalidCatalogException(errorMessages);
        }

        JArray categoryItems = (JArray)catalog["Category"]!;
        foreach (var item in categoryItems.OfType<JObject>())
        {
            string name = item.Value<string>("Name") ?? "";
            var discount = item.Value<decimal>("Discount");

            CategoryDiscounts[name] = discount;
        }

        JArray catalogItems = (JArray)catalog["Catalog"]!;
        foreach (var item in catalogItems.OfType<JObject>())
        {
            string name = item.Value<string>("Name") ?? "";
            string category = item.Value<string>("Category") ?? "";
            decimal price = item.Value<decimal>("Price");
            int quantity = item.Value<int>("Quantity");

            books.Add(name, new Book(name, category, price, quantity));
        }
    }

    internal bool ContainsBook(string name, string category, decimal price, int quantity)
    {
        return books.ContainsKey(name)
            && books[name].Name.Equals(name)
            && books[name].Category.Equals(category)
            && books[name].Quantity == quantity
            && books[name].Price == price;
    }

    internal int GetBooksCount() => books.Count();

    private decimal CalculatePrice(IEnumerable<Book> books)
    {
        decimal price = 0;

        var cetegoryGroup = books.GroupBy(b => b.Category);

        foreach (var booksFromCategory in cetegoryGroup)
        {
            decimal categoryDiscount = CategoryDiscounts.ContainsKey(booksFromCategory.Key) ? CategoryDiscounts[booksFromCategory.Key] : 1;

            if (booksFromCategory.Count() > 1)
            {
                //2.If the customer buys several books then a discount applies if
                //these books belong to the same category
                var booksByName = booksFromCategory.GroupBy(el => el.Name);

                foreach (var book in booksByName)
                {
                    var discount = (decimal)categoryDiscount;

                    foreach (var copy in book)
                    {
                        //3.Only the first copy of each book has the right to the reduction
                        price += (discount * copy.Price) + ((copy.Quantity - 1) * copy.Price);
                        discount = 1;
                    }
                }
            }
            else
            {
                // 1.The purchase of a single book is paid at the price of the book provided in the catalog.
                price += booksFromCategory.Sum(b => b.Price * b.Quantity);
            }

        }

        return price;
    }

    private IEnumerable<Book> GetBooksFromStock(Dictionary<string, int> booksFromBasket)
    {
        List<Book> bought = new List<Book>();

        foreach (var book in booksFromBasket)
        {
            Book bookToBuy = books[book.Key];
            bought.Add(new Book(bookToBuy.Name, bookToBuy.Category, bookToBuy.Price, book.Value));
            TryDecreaseQuantity(book.Key, book.Value);
        }

        return bought;
    }

    private bool TryDecreaseQuantity(string name, int quantityToDecrease)
    {
        if (!books.TryGetValue(name, out var book))
        {
            return false;
        }

        int newQuantity = book.Quantity - quantityToDecrease;
        if (newQuantity < 0)
        {
            return false;
        }

        books.Remove(name);
        if (newQuantity != 0)
        {
            var newBook = new Book(book.Name, book.Category, book.Price, newQuantity);
            books.Add(name, newBook);
        }

        return true;
    }

    private bool ValidateBooksQuantities(Dictionary<string, int> booksFromBasket, out List<NotEnoughInventoryException.NameQuantity> validationErrors)
    {
        validationErrors = new();
        bool isValid = true;

        foreach (var book in booksFromBasket)
        {
            if (!books.TryGetValue(book.Key, out var found))
            {
                validationErrors.Add(new NotEnoughInventoryException.NameQuantity(book.Key, book.Value));
                continue;
            }

            var availableQuantity = found.Quantity;
            if (availableQuantity < book.Value)
            {
                validationErrors.Add(new NotEnoughInventoryException.NameQuantity(book.Key, book.Value - availableQuantity));
                isValid = false;
            }
        }

        return isValid;
    }
}