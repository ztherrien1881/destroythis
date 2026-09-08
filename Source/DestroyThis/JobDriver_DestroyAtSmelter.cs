using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace DestroyThis
{
    public sealed class JobDriver_DestroyAtSmelter : JobDriver
    {
        private const TargetIndex ItemInd = TargetIndex.A;
        private const TargetIndex SmelterInd = TargetIndex.B;
        private const int WorkTicks = 300;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(ItemInd), job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(job.GetTarget(SmelterInd), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(ItemInd);
            this.FailOnDestroyedNullOrForbidden(SmelterInd);
            this.FailOn(delegate
            {
                Building_WorkTable table = job.GetTarget(SmelterInd).Thing as Building_WorkTable;
                return table == null || !table.CurrentlyUsableForBills();
            });

            yield return Toils_Goto.GotoThing(ItemInd, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(ItemInd);
            yield return Toils_Haul.StartCarryThing(ItemInd, false, true, false);
            yield return Toils_Goto.GotoThing(SmelterInd, PathEndMode.InteractionCell);

            Toil work = Toils_General.Wait(WorkTicks, SmelterInd);
            work.WithProgressBarToilDelay(SmelterInd);
            yield return work;

            Toil destroy = ToilMaker.MakeToil("DestroyDesignatedItem");
            destroy.initAction = () =>
            {
                Thing carried = pawn.carryTracker.CarriedThing;
                RecipeDef recipe;
                if (carried != null && DestroyThisUtility.TryGetDestroyRecipe(carried, out recipe))
                {
                    Building_WorkTable smelter = job.GetTarget(SmelterInd).Thing as Building_WorkTable;
                    List<Thing> ingredients = new List<Thing> { carried };
                    List<Thing> products = GenRecipe.MakeRecipeProducts(recipe, pawn, ingredients, carried, smelter).ToList();
                    recipe.Worker.ConsumeIngredient(carried, recipe, pawn.Map);
                    for (int i = 0; i < products.Count; i++)
                        GenPlace.TryPlaceThing(products[i], smelter.InteractionCell, pawn.Map, ThingPlaceMode.Near);
                }
            };
            destroy.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return destroy;
        }
    }
}