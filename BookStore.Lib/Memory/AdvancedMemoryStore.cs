namespace BookStore.Lib.Memory;

internal class AdvancedMemoryStore : IStore
{
    internal record Book(string Name, string Category, decimal Price) { public int Quantity { get; set; } }

    internal readonly BookStorage _bookStorage = new();

    public Dictionary<string, decimal> Categories { get; } = new();

    public decimal Buy(params string[] basketByNames)
    {
        var booksFromBasket = basketByNames.GroupBy(x => x).ToDictionary(x => x.Key, g => g.Count());

        if (!ValidateBooksQuantities(booksFromBasket, out var validationErrors))
        {
            throw new NotEnoughInventoryException(validationErrors);
        }

        var books = GetBooksFromStock(booksFromBasket);

        return CalculatePrice(books);
    }

    public int GetQuantity(string name) => _bookStorage.GetTotalQuantity(name);

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

            Categories[name] = discount;
        }

        JArray catalogItems = (JArray)catalog["Catalog"]!;
        foreach (var item in catalogItems.OfType<JObject>())
        {
            string name = item.Value<string>("Name") ?? "";
            string category = item.Value<string>("Category") ?? "";
            decimal price = item.Value<decimal>("Price");
            int quantity = item.Value<int>("Quantity");
            _bookStorage.Add(name, category, price, quantity);
        }
    }

    private decimal CalculatePrice(List<Book> books)
    {
        decimal price = 0;

        var cetegoryGroup = books.GroupBy(b => b.Category);

        foreach (var booksFromCategory in cetegoryGroup)
        {
            decimal categoryDiscount = Categories.ContainsKey(booksFromCategory.Key) ? Categories[booksFromCategory.Key] : 1;

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
                price += booksFromCategory.Sum(b => b.Price *  b.Quantity);
            }

        }

        return price;
    }

    private bool ValidateBooksQuantities(Dictionary<string, int> booksFromBasket, out List<NotEnoughInventoryException.NameQuantity> validationErrors)
    {
        validationErrors = new();
        bool isValid = true;

        foreach (var book in booksFromBasket)
        {
            var availableQuantity = 0;

            var found = _bookStorage.GetBooksByName(book.Key);
            availableQuantity = found.Sum(book => book.Quantity);

            if (availableQuantity == 0 || availableQuantity - book.Value < 0)
            {
                //4.If a shopping cart is invalid because the catalog does not contain enough books
                //the owner expects to receive a NotEnoughInventoryException containing
                validationErrors.Add(new NotEnoughInventoryException.NameQuantity(book.Key, Math.Abs(availableQuantity - book.Value)));
                isValid = false;
            }
        }
        return isValid;
    }

    private List<Book> GetBooksFromStock(Dictionary<string, int> booksFromBasket)
    {
        List<Book> books = new List<Book>();

        foreach (var book in booksFromBasket)
        {
            var availableQuantity = 0;

            var found = _bookStorage.GetBooksByName(book.Key);
            availableQuantity = found.Sum(book => book.Quantity);
            found.OrderBy(c => c.Price);

            int bookNeededCopies = book.Value;
            foreach (var availableBook in found.ToList())
            {
                if (availableBook.Quantity <= bookNeededCopies)
                {
                    bookNeededCopies -= availableBook.Quantity;
                    books.Add(new Book(availableBook.Name, availableBook.Category, availableBook.Price) { Quantity = availableBook.Quantity });

                    _bookStorage.TryDecreaseQuantity(availableBook.Name, availableBook.Category, availableBook.Price, availableBook.Quantity);
                }
                else
                {
                    books.Add(new Book(availableBook.Name, availableBook.Category, availableBook.Price) { Quantity = bookNeededCopies });
                    _bookStorage.TryDecreaseQuantity(availableBook.Name, availableBook.Category, availableBook.Price, bookNeededCopies);

                    break;
                }
            }
        }

        return books;
    }

    internal class BookStorage
    {
        protected readonly Dictionary<string, List<Book>> _booksByName = new();

        public void Add(string name, string category, decimal price, int quantity)
        {

            var newBook = new Book(name, category, price)
            {
                Quantity = quantity
            };

            if (!_booksByName.TryGetValue(name, out List<Book>? books))
            {
                _booksByName[name] = books = new List<Book>() { newBook };
                return;
            }

            var found = books.FirstOrDefault(b => b.Category.Equals(category)
                                               && b.Price.Equals(price));
            if (found != null)
            {
                found.Quantity += quantity;
                return;
            }

            _booksByName[name].Add(newBook);
        }

        public bool TryDecreaseQuantity(string name, string category, decimal price, int quantityToDecrease)
        {
            if (!_booksByName.TryGetValue(name, out List<Book>? books))
            {
                return false;
            }

            var newBook = new Book(name, category, price);

            var found = books.FirstOrDefault(b => b.Price.Equals(price)
                                               && b.Category.Equals(category));
            if (found == null)
            {
                return false;
            }

            int left = found.Quantity - quantityToDecrease;

            if (left < 0)
            {
                return false;
            }

            found.Quantity = left;

            if (left == 0)
            {
                _booksByName[name].Remove(newBook);
                if (_booksByName[name].Count() == 0)
                {
                    _booksByName.Remove(name);
                }
            }

            return true;
        }

        public int GetTotalQuantity(string name)
        {
            if (!_booksByName.TryGetValue(name, out List<Book>? books))
            {
                return 0;
            }

            return books.Sum(book => book.Quantity);
        }

        internal IEnumerable<Book> GetBooksByName(string name)
        {
            if (!_booksByName.TryGetValue(name, out List<Book>? books))
            {
                return Array.Empty<Book>();
            }

            return books;
        }

        internal IEnumerable<(string category, decimal price, int quantity)> GetBooksPropertiesByName(string name)
        {
            if (!_booksByName.TryGetValue(name, out List<Book>? books))
            {
                return Array.Empty<(string category, decimal price, int quantity)>();
            }

            return books.Select(book => (book.Category, book.Price, book.Quantity));
        }
    }
}