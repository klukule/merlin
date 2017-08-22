namespace Merlin
{
    public static class SilverObjectViewExtensions
    {
        static SilverObjectViewExtensions()
        {

        }

        public static bool IsLootProtected(this SilverObjectView instance)
        {
            return !instance.SilverObject.sf();
        }
    }
}