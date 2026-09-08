using HarmonyLib;
using Verse;

namespace DestroyThis
{
    [StaticConstructorOnStartup]
    public static class DestroyThisMod
    {
        static DestroyThisMod()
        {
            new Harmony("Zach.DestroyThis").PatchAll();
        }
    }
}