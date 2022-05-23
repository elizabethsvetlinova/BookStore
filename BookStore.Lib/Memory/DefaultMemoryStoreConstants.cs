namespace BookStore.Lib.Memory;

internal static class DefaultMemoryStoreConstants
{
    internal const string Schema =
@"{
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