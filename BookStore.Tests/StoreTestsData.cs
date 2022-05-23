namespace BookStore.Tests
{
    internal static class StoreTestsData
    {
        public const string Stock = @"
{
  ""Category"": [
    {
      ""Name"": ""Science Fiction"",
      ""Discount"": 0.05
    },
    {
      ""Name"": ""Fantastique"",
      ""Discount"": 0.1
    },
    {
      ""Name"": ""Philosophy"",
      ""Discount"": 0.15
    }
  ],
  ""Catalog"": [
    {
      ""Name"": ""J.K Rowling - Goblet Of fire"",
      ""Category"": ""Fantastique"",
      ""Price"": 8,
      ""Quantity"": 2
    },
    {
      ""Name"": ""Ayn Rand - FountainHead"",
      ""Category"": ""Philosophy"",
      ""Price"": 12,
      ""Quantity"": 10
    },
    {
    ""Name"": ""Isaac Asimov - Foundation"",
      ""Category"": ""Science Fiction"",
      ""Price"": 16,
      ""Quantity"": 1
    },
    {
    ""Name"": ""Isaac Asimov - Robot series"",
      ""Category"": ""Science Fiction"",
      ""Price"": 5,
      ""Quantity"": 1
    },
    {
    ""Name"": ""Robin Hobb - Assassin Apprentice"",
      ""Category"": ""Fantastique"",
      ""Price"": 12,
      ""Quantity"": 8
    }
  ]
}

";


        public const string Schema = @"{
          ""type"": ""object"",
          ""title"": ""The Root Schema"",
          ""required"": [""Category"", ""Catalog""],
          ""properties"": {
            ""Category"": {
              ""type"": ""array"",
              ""title"": ""List of existing category with associated discount"",
              ""items"": {
                ""type"": ""object"",
                ""title"": ""one category with its discount"",
                ""required"": [""Name"", ""Discount""],
                ""properties"": {
                  ""Name"": {
                    ""type"": ""string"",
                    ""title"": ""The unique name of the category, it is a functionnal key"",

                    ""default"": """",
                    ""examples"": [""Fantastique""],
                    ""pattern"": ""^(.+)$""
                  },
                  ""Discount"": {
                    ""type"": ""number"",
                    ""title"": ""the discount applies when buying multiple book of this category"",

                    ""default"": 0.0,
                    ""examples"": [0.05]
                  }
                }
              }
            },
            ""Catalog"": {
            ""type"": ""array"",
              ""title"": ""The Catalog of the store"",
              ""items"": {
                ""type"": ""object"",
                ""title"": ""a book in the catalog"",
                ""required"": [""Name"", ""Category"", ""Price"", ""Quantity""],
                ""properties"": {
                    ""Name"": {
                        ""type"": ""string"",
                    ""title"": ""The unique Name of the book, it is a functionnal key"",
                    ""default"": """",
                    ""examples"": [""J.K Rowling - Goblet Of fire""],
                    ""pattern"": ""^(.+)$""
                    },
                  ""Category"": {
                        ""type"": ""string"",
                    ""title"": ""The name of one the category existing in the Category root properties."",
                    ""default"": """",
                    ""examples"": [""Fantastique""],
                    ""pattern"": ""^(.+)$""
                  },
                  ""Price"": {
                        ""type"": ""number"",
                    ""title"": ""the price of an copy of the book"",
                    ""default"": 0,
                    ""examples"": [8]
                  },
                  ""Quantity"": {
                        ""type"": ""integer"",
                    ""title"": ""The Quantity of copy of the book in the catalog."",
                    ""default"": 0,
                    ""examples"": [2]
                  }
                }
            }
        }
          }
        }";
    }
}