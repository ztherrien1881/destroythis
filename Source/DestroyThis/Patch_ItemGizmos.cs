using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DestroyThis
{
    [HarmonyPatch(typeof(ThingWithComps), "GetGizmos")]
    public static class Patch_ItemGizmos
    {
        public static void Postfix(ThingWithComps __instance, ref IEnumerable<Gizmo> __result)
        {
            DesignationDef designationDef = DestroyThisUtility.DestroyDesignationDef;
            if (__instance == null || !__instance.Spawned || __instance.Map == null || designationDef == null) return;

            Designation designation = __instance.Map.designationManager.DesignationOn(__instance, designationDef);
            if (designation == null)
            {
                RecipeDef recipe;
                if (!DestroyThisUtility.TryGetDestroyRecipe(__instance, out recipe)) return;
            }

            __result = (__result ?? Enumerable.Empty<Gizmo>()).Concat(GizmosFor(__instance, designationDef));
        }

        private static IEnumerable<Gizmo> GizmosFor(Thing item, DesignationDef designationDef)
        {
            Designation designation = item.Map.designationManager.DesignationOn(item, designationDef);
            Command_Action command = new Command_Action
            {
                defaultLabel = designation == null ? "DestroyThis.CommandLabel".Translate() : "DestroyThis.CancelCommandLabel".Translate(),
                defaultDesc = designation == null ? "DestroyThis.CommandDesc".Translate() : "DestroyThis.CancelCommandDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/DestroyThis")
            };

            command.action = delegate
            {
                if (item == null || !item.Spawned || item.Map == null) return;

                Designation current = item.Map.designationManager.DesignationOn(item, designationDef);
                if (current == null)
                {
                    RecipeDef recipe;
                    if (DestroyThisUtility.TryGetDestroyRecipe(item, out recipe))
                        item.Map.designationManager.AddDesignation(new Designation(item, designationDef));
                }
                else
                {
                    item.Map.designationManager.RemoveDesignation(current);
                }
            };

            yield return command;
        }
    }
}
