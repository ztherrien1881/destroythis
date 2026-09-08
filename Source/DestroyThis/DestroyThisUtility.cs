using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace DestroyThis
{
    internal static class DestroyThisUtility
    {
        private const string ElectricSmelterDefName = "ElectricSmelter";
        private const string DestroyDesignationDefName = "DestroyThis_Destroy";
        private const string DestroyJobDefName = "DestroyThis_DestroyAtSmelter";

        internal static ThingDef ElectricSmelterDef
        {
            get { return DefDatabase<ThingDef>.GetNamedSilentFail(ElectricSmelterDefName); }
        }

        internal static DesignationDef DestroyDesignationDef
        {
            get { return DefDatabase<DesignationDef>.GetNamedSilentFail(DestroyDesignationDefName); }
        }

        internal static JobDef DestroyJobDef
        {
            get { return DefDatabase<JobDef>.GetNamedSilentFail(DestroyJobDefName); }
        }

        internal static bool IsQuestItem(Thing thing)
        {
            return thing.questTags != null && thing.questTags.Count > 0;
        }

        internal static bool IsRelic(Thing thing)
        {
            ThingWithComps twc = thing as ThingWithComps;
            return twc != null && twc.AllComps != null && twc.AllComps.Any(comp => comp.GetType().Name == "CompRelic");
        }

        internal static bool TryGetDestroyRecipe(Thing thing, out RecipeDef recipe)
        {
            recipe = null;
            ThingDef smelterDef = ElectricSmelterDef;
            if (thing == null || thing.def.category != ThingCategory.Item || smelterDef == null) return false;
            IEnumerable<RecipeDef> recipes = smelterDef.AllRecipes;
            if (recipes == null || IsQuestItem(thing) || IsRelic(thing)) return false;

            recipe = recipes
                .Where(candidate => IsDisposalRecipe(candidate) && Allows(candidate, thing))
                .OrderBy(candidate => candidate.specialProducts == null || candidate.specialProducts.Count == 0 ? 1 : 0)
                .FirstOrDefault();
            return recipe != null;
        }

        private static bool IsDisposalRecipe(RecipeDef recipe)
        {
            return recipe != null && !recipe.IsSurgery && recipe.ingredients != null && recipe.ingredients.Count == 1 &&
                   (recipe.products == null || recipe.products.Count == 0);
        }

        private static bool Allows(RecipeDef recipe, Thing thing)
        {
            if (recipe.fixedIngredientFilter != null && !recipe.fixedIngredientFilter.Allows(thing)) return false;
            return recipe.ingredients[0].filter.Allows(thing);
        }

        internal static Building_WorkTable FindSmelter(Pawn pawn, Thing item)
        {
            ThingDef smelterDef = ElectricSmelterDef;
            if (pawn == null || pawn.Map == null || item == null || smelterDef == null) return null;

            return GenClosest.ClosestThingReachable(
                item.Position, item.Map, ThingRequest.ForDef(smelterDef), PathEndMode.InteractionCell,
                TraverseParms.For(pawn), 9999f,
                delegate(Thing candidate)
                {
                    Building_WorkTable table = candidate as Building_WorkTable;
                    return table != null && table.Faction == pawn.Faction &&
                           table.CurrentlyUsableForBills() && pawn.CanReserve(table);
                }) as Building_WorkTable;
        }
    }
}