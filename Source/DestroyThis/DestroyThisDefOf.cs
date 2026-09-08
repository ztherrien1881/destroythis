using RimWorld;
using Verse;

namespace DestroyThis
{
    [DefOf]
    public static class DestroyThisDefOf
    {
        public static DesignationDef DestroyThis_Destroy;
        public static JobDef DestroyThis_DestroyAtSmelter;

        static DestroyThisDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DestroyThisDefOf));
        }
    }
}