using System;
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
            InstallOrdersDesignator();
        }

        private static void InstallOrdersDesignator()
        {
            DesignationCategoryDef orders = DefDatabase<DesignationCategoryDef>.GetNamedSilentFail("Orders");
            if (orders == null)
            {
                Log.Error("[Destroy This] Could not find the vanilla Orders designation category.");
                return;
            }

            Type designatorType = typeof(Designator_DestroyThis);
            if (!orders.specialDesignatorClasses.Contains(designatorType))
                orders.specialDesignatorClasses.Add(designatorType);

            var resolveDesignators = AccessTools.Method(typeof(DesignationCategoryDef), "ResolveDesignators");
            if (resolveDesignators == null)
            {
                Log.Error("[Destroy This] Could not resolve DesignationCategoryDef.ResolveDesignators.");
                return;
            }

            resolveDesignators.Invoke(orders, null);
            Log.Message("[Destroy This] Added Destroy this to Architect > Orders.");
        }
    }
}