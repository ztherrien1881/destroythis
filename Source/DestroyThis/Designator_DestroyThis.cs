using System.Collections.Generic;
using RimWorld;
using Verse;

namespace DestroyThis
{
    public sealed class Designator_DestroyThis : Designator
    {
        public Designator_DestroyThis()
        {
            defaultLabel = DestroyThisDefOf.DestroyThis_Destroy.label;
            defaultDesc = DestroyThisDefOf.DestroyThis_Destroy.description;
            icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/Designators/Deconstruct");
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_Deconstruct;
        }

        public override AcceptanceReport CanDesignateThing(Thing thing)
        {
            if (thing == null || !thing.Spawned || thing.def.category != ThingCategory.Item)
                return "DestroyThis.MustBeLooseItem".Translate();

            if (thing.Map.designationManager.DesignationOn(thing, DestroyThisDefOf.DestroyThis_Destroy) != null)
                return false;

            if (DestroyThisUtility.IsQuestItem(thing))
                return "DestroyThis.QuestItem".Translate();

            if (DestroyThisUtility.IsRelic(thing))
                return "DestroyThis.Relic".Translate();

            RecipeDef recipe;
            return DestroyThisUtility.TryGetDestroyRecipe(thing, out recipe)
                ? AcceptanceReport.WasAccepted
                : new AcceptanceReport("DestroyThis.NotDestroyable".Translate());
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 cell)
        {
            Map map = Find.CurrentMap;
            if (map == null || !cell.InBounds(map)) return false;
            List<Thing> things = cell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
                if (CanDesignateThing(things[i]).Accepted) return AcceptanceReport.WasAccepted;
            return new AcceptanceReport("DestroyThis.NotDestroyable".Translate());
        }

        public override void DesignateSingleCell(IntVec3 cell)
        {
            Map map = Find.CurrentMap;
            if (map == null || !cell.InBounds(map)) return;
            List<Thing> things = new List<Thing>(cell.GetThingList(map));
            for (int i = 0; i < things.Count; i++)
                if (CanDesignateThing(things[i]).Accepted) DesignateThing(things[i]);
        }

        public override void DesignateThing(Thing thing)
        {
            thing.Map.designationManager.AddDesignation(new Designation(thing, DestroyThisDefOf.DestroyThis_Destroy));
        }
    }
}