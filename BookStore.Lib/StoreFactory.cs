namespace BookStore.Lib;

public static class StoreFactory
{
    public static IStore CreateInMemoryAdvanced() => new Memory.AdvancedMemoryStore();

    public static IStore CreateInMemorySimple() => new Memory.SimpleMemoryStore();
}
