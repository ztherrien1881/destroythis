using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace DestroyThis
{
    public sealed class WorkGiver_DestroyThis : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode { get { return PathEndMode.ClosestTouch; } }
        public override Danger MaxPathDanger(Pawn pawn) { return Danger.Some; }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null) return Enumerable.Empty<Thing>();
            return pawn.Map.designationManager.SpawnedDesignationsOfDef(DestroyThisDefOf.DestroyThis_Destroy)
                .Select(d => d.target.Thing).Where(t => t != null);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            return thing.Spawned && !thing.IsForbidden(pawn) &&
                   pawn.CanReserveAndReach(thing, PathEndMode.ClosestTouch, Danger.Some) &&
                   HasDestroyRecipe(thing) && DestroyThisUtility.FindSmelter(pawn, thing) != null;
        }

        private static bool HasDestroyRecipe(Thing thing)
        {
            RecipeDef recipe;
            return DestroyThisUtility.TryGetDestroyRecipe(thing, out recipe);
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Building_WorkTable smelter = DestroyThisUtility.FindSmelter(pawn, thing);
            if (smelter == null) return null;
            Job job = JobMaker.MakeJob(DestroyThisDefOf.DestroyThis_DestroyAtSmelter, thing, smelter);
            job.count = thing.stackCount;
            return job;
        }
    }
}