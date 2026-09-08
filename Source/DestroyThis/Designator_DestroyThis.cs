using System.Collections.Generic;
using RimWorld;
using Verse;

namespace DestroyThis
{
    public sealed class Designator_DestroyThis : Designator
    {
        public Designator_DestroyThis()
        {
            DesignationDef def = DestroyThisUtility.DestroyDesignationDef;
            defaultLabel = def != null ? def.label : "Destroy this";
            defaultDesc = def != null ? def.description : "Designate an item for processing at an electric smelter.";
            icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/Commands/DestroyThis");
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_Deconstruct;
        }

        public override AcceptanceReport CanDesignateThing(Thing thing)
        {
            DesignationDef designationDef = DestroyThisUtility.DestroyDesignationDef;
            if (designationDef == null) return false;

            if (thing == null || !thing.Spawned || thing.def.category != ThingCategory.Item)
                return "DestroyThis.MustBeLooseItem".Translate();

            if (thing.Map.designationManager.DesignationOn(thing, designationDef) != null)
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
            DesignationDef designationDef = DestroyThisUtility.DestroyDesignationDef;
            if (designationDef != null)
                thing.Map.designationManager.AddDesignation(new Designation(thing, designationDef));
        }
    }
}
