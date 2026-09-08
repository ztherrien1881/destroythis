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
            if (__instance == null || !__instance.Spawned || __instance.Map == null) return;

            Designation designation = __instance.Map.designationManager.DesignationOn(__instance, DestroyThisDefOf.DestroyThis_Destroy);
            if (designation == null)
            {
                RecipeDef recipe;
                if (!DestroyThisUtility.TryGetDestroyRecipe(__instance, out recipe)) return;
            }

            __result = (__result ?? Enumerable.Empty<Gizmo>()).Concat(GizmosFor(__instance));
        }

        private static IEnumerable<Gizmo> GizmosFor(Thing item)
        {
            Designation designation = item.Map.designationManager.DesignationOn(item, DestroyThisDefOf.DestroyThis_Destroy);
            Command_Action command = new Command_Action
            {
                defaultLabel = designation == null ? "DestroyThis.CommandLabel".Translate() : "DestroyThis.CancelCommandLabel".Translate(),
                defaultDesc = designation == null ? "DestroyThis.CommandDesc".Translate() : "DestroyThis.CancelCommandDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct")
            };

            command.action = delegate
            {
                if (item == null || !item.Spawned || item.Map == null) return;
                Designation current = item.Map.designationManager.DesignationOn(item, DestroyThisDefOf.DestroyThis_Destroy);
                if (current == null)
                {
                    RecipeDef recipe;
                    if (DestroyThisUtility.TryGetDestroyRecipe(item, out recipe))
                        item.Map.designationManager.AddDesignation(new Designation(item, DestroyThisDefOf.DestroyThis_Destroy));
                }
                else item.Map.designationManager.RemoveDesignation(current);
            };

            yield return command;
        }
    }
}