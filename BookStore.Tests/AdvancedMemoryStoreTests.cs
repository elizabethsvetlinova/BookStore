namespace BookStore.Tests
{
    public partial class AdvancedMemoryStoreTests
    {
        record Category(string Name, decimal Discount);

        record Book(string Name, string Category, int Price, int Quantity);
       
        [Fact]
        public void TestStoreImport()
        {
            AdvancedMemoryStore store = new AdvancedMemoryStore();
            store.Import(StoreTestsData.Stock);

            Assert.Equal(3, store.Categories.Count);
            Assert.Equal(0.05m, store.Categories["Science Fiction"]);
            Assert.Equal(0.1m, store.Categories["Fantastique"]);
            Assert.Equal(0.15m, store.Categories["Philosophy"]);

            Assert.Equal(("Fantastique", 8.0m, 2), store._bookStorage.GetBooksPropertiesByName("J.K Rowling - Goblet Of fire").Single());
            Assert.Equal(("Philosophy", 12.0m, 10), store._bookStorage.GetBooksPropertiesByName("Ayn Rand - FountainHead").Single());
            Assert.Equal(("Science Fiction", 16.0m, 1), store._bookStorage.GetBooksPropertiesByName("Isaac Asimov - Foundation").Single());
            Assert.Equal(("Science Fiction", 5.0m, 1), store._bookStorage.GetBooksPropertiesByName("Isaac Asimov - Robot series").Single());
            Assert.Equal(("Fantastique", 12.0m, 8), store._bookStorage.GetBooksPropertiesByName("Robin Hobb - Assassin Apprentice").Single());
        }

        [Theory]
        [InlineData("Isaac Asimov - Foundation", 1)] //check for existing book in stock
        [InlineData("Issac", 0)]                      //check for unexisting book in stock -> should return 0
        public void TestStoreQuantityForBook(string bookTitle, int quantity)
        {
            IStore store = StoreFactory.CreateInMemoryAdvanced();
            store.Import(StoreTestsData.Stock);
            Assert.Equal(quantity, store.GetQuantity(bookTitle));
        }

        [Fact]
        public void TestStoreBuy()
        {
            string[] books =
                {
                "J.K Rowling - Goblet Of fire", "J.K Rowling - Goblet Of fire",
                "Robin Hobb - Assassin Apprentice","Robin Hobb - Assassin Apprentice",
                "Ayn Rand - FountainHead",
                "Isaac Asimov - Foundation",
                "Isaac Asimov - Robot series"
             };
            AdvancedMemoryStore store = new AdvancedMemoryStore();
            store.Import(StoreTestsData.Stock);

            var price = store.Buy(books);

            decimal expectedPrice = (0.1m * 8m + 8m + 0.1m * 12m + 12m) + (0.05m * 16m + 0.05m * 5m) + 12m;
            Assert.Equal(expectedPrice, price);
            
            Assert.Equal(("Philosophy", 12.0m, 9), store._bookStorage.GetBooksPropertiesByName("Ayn Rand - FountainHead").Single());
            Assert.Equal(("Fantastique", 12.0m, 6), store._bookStorage.GetBooksPropertiesByName("Robin Hobb - Assassin Apprentice").Single());
        }

        [Fact]
        public void TestStoreBuyWithSeveralCategoryCases()
        {
            AdvancedMemoryStore store = new();
            List<Book> catalogs = new List<Book>();
            List<Category> categories = new List<Category>();

            catalogs.Add(new Book("book1", "category-1", 4, 3));
            catalogs.Add(new Book("book2", "category-1", 6, 3));

            catalogs.Add(new Book("book3", "category-2", 4, 2));

            catalogs.Add(new Book("book4", "category-3", 4, 1));
            catalogs.Add(new Book("book5", "category-3", 5, 1));
            catalogs.Add(new Book("book6", "category-3", 6, 3));

            catalogs.Add(new Book("book7", "category-4", 4, 1));

            catalogs.Add(new Book("book8", "category-5", 5, 2));
            catalogs.Add(new Book("book9", "category-5", 6, 1));
            catalogs.Add(new Book("book10", "category-5", 6, 1));



            categories.Add(new Category("category-1", 0.5m));
            categories.Add(new Category("category-2", 0.8m));
            categories.Add(new Category("category-3", 0.9m));
            categories.Add(new Category("category-4", 0.2m));

            var storeJson = GenerateImportData(catalogs, categories);
            store.Import(storeJson);

            string[] books =
            {
                "book1", "book1","book1", "book2", "book2","book2", //full all categories
                "book3","book3",//full one category
                "book4","book5","book6", //some categories
                "book8", //only one -> no discount
            };
            var expectedPrice =
                +(4m * 0.5m) + 4m + 4m + (6m * 0.5m) + 6m + 6m
                + 4m + 4m
                + (4m * 0.9m) + (5m * 0.9m) + (6m * 0.9m)
                + 0m
                + 5m;
            decimal price = store.Buy(books);
            Assert.Equal(expectedPrice, price);
        }

        [Fact]
        public void BuyingBooksFromOneCategoryShouldFailIfNotEnoughtQuantity()
        {
            AdvancedMemoryStore store = new();
            List<Book> catalogs = new List<Book>();
            List<Category> categories = new List<Category>();

            catalogs.Add(new Book("book1", "category-1", 4, 3));

            categories.Add(new Category("category-1", 0.5m));

            var storeJson = GenerateImportData(catalogs, categories);
            store.Import(storeJson);

            string[] booksToBuy =
            {
                "book1", "book1","book1","book1",
            };

            Action act = () => { float price = (float)store.Buy(booksToBuy); };
            var exception = Assert.Throws<NotEnoughInventoryException>(act);

            Assert.Single(exception.Missing);

            INameQuantity? missing = exception.Missing.First();
            Assert.Equal("book1", missing.Name);
            Assert.Equal(1, missing.Quantity);
        }

        [Fact]
        public void BuyingBooksFromMoreThanOneCategoryShouldFailIfNotEnoughtQuantity()
        {

            AdvancedMemoryStore store = new();
            List<Book> catalogs = new List<Book>();
            List<Category> categories = new List<Category>();

            catalogs.Add(new Book("book1", "category-1", 4, 3));
            catalogs.Add(new Book("book2", "category-2", 4, 1));
            catalogs.Add(new Book("book3", "category-3", 4, 1));

            var storeJson = GenerateImportData(catalogs, categories);
            store.Import(storeJson);

            string[] booksToBuy =
            {
                "book1", "book1","book1","book1",
                "book2", "book2","book2","book2",
                "book3",
                "book4"
            };

            Action act = () => { float price = (float)store.Buy(booksToBuy); };
            var exception = Assert.Throws<NotEnoughInventoryException>(act);

            var missings = exception.Missing.ToList();

            Assert.Equal(3, missings.Count());

            Assert.Equal("book1", missings[0].Name);
            Assert.Equal(1, missings[0].Quantity);
            Assert.Equal("book2", missings[1].Name);
            Assert.Equal(3, missings[1].Quantity);
            Assert.Equal("book4", missings[2].Name);
            Assert.Equal(1, missings[2].Quantity);
        }

        [Fact]
        public void BuyingOneBookFromOneCategory()
        {
            AdvancedMemoryStore store = new();
            List<Book> catalogs = new List<Book>();
            List<Category> categories = new List<Category>();

            catalogs.Add(new Book("book1", "category-1", 4, 3));

            categories.Add(new Category("category-1", 0.5m));

            var storeJson = GenerateImportData(catalogs, categories);
            store.Import(storeJson);

            string[] booksToBuy =
            {
                "book1"
            };

            var price = store.Buy(booksToBuy); //buy once and check available quantity

            Assert.Equal(4, price);
            Assert.Equal(("category-1", 4m, 2), store._bookStorage.GetBooksPropertiesByName("book1").Single());


            price = store.Buy(booksToBuy);
            Assert.Equal(4, price);
            Assert.Equal(("category-1", 4m, 1), store._bookStorage.GetBooksPropertiesByName("book1").Single());

            price = store.Buy(booksToBuy);
            Assert.Equal(4, price);
            //Assert.Empty(store._bookStorage.GetTotalQuantity());
        }

        [Fact]
        public void BuyingBooksFromOneCategory()
        {
            AdvancedMemoryStore store = new AdvancedMemoryStore();
            List<Book> catalogs = new List<Book>();
            List<Category> categories = new List<Category>();

            catalogs.Add(new Book("book1", "category-1", 4, 3));
            catalogs.Add(new Book("book2", "category-1", 5, 3));
            catalogs.Add(new Book("book2", "category-1", 8, 3));

            categories.Add(new Category("category-1", 0.5m));

            var storeJson = GenerateImportData(catalogs, categories);
            store.Import(storeJson);

            string[] booksToBuy =
            {
                "book1","book1",
                "book2"
            };

            var expectedPrice = (4m * 0.5m + 4m) + (5m * 0.5m);
            var price = store.Buy(booksToBuy); //buy once and check available quantity

            Assert.Equal(expectedPrice, price);
            //Assert.Equal(3, store._bookStorage.GetTotalQuantity.Count());
            Assert.Equal(("category-1", 4m, 1), store._bookStorage.GetBooksPropertiesByName("book1").Single());

            var bookFromStorage = store._bookStorage.GetBooksPropertiesByName("book2").ToArray();
            Assert.Equal(("category-1", 5m, 2), bookFromStorage[0]);
            Assert.Equal(("category-1", 8m, 3), bookFromStorage[1]);

            string[] secondBuyList = { "book1", "book2", "book2" };
            price = store.Buy(secondBuyList);
            Assert.Equal(4m * 0.5m + 0.5m * 5m + 5m, price);
            //Assert.Single(store.Catalogs);
            Assert.Equal(("category-1", 8m, 3), store._bookStorage.GetBooksPropertiesByName("book2").Single());

            price = store.Buy("book2");
            Assert.Equal(8, price);
        }

        private string GenerateImportData(List<Book> catalogs, List<Category> categories)
        {
            return JsonConvert.SerializeObject(new Dictionary<string, object>() { { "Category", categories.ToArray() }, { "Catalog", catalogs.ToArray() } });
        }
    }
}