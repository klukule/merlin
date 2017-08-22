namespace Merlin
{
    public static class LootObjectViewExtensions
    {
        static LootObjectViewExtensions()
        {

        }

        public static bool IsLootProtected(this LootObjectView instance)
        {
            return !instance.LootObject.mh();
        }
    }
}