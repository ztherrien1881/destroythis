using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Minisplits
{
    // Rotation follows vanilla coolers: local south is the conditioned side.
    internal static class MinisplitPlacement
    {
        public static List<IntVec3> Ports(ThingDef def, IntVec3 center, Rot4 rotation, bool exhaust)
        {
            var result = new List<IntVec3>();
            IntVec3 offset = (exhaust ? IntVec3.North : IntVec3.South).RotatedBy(rotation);
            foreach (IntVec3 cell in GenAdj.OccupiedRect(center, rotation, def.size))
                result.Add(cell + offset);
            return result;
        }

        public static bool HasWall(IntVec3 cell, Map map)
        {
            if (!cell.InBounds(map)) return false;
            Building wall = cell.GetEdifice(map);
            return wall != null && !(wall is Building_Door) &&
                (wall.def.building.isPlaceOverableWall || wall.def.IsSmoothed);
        }

        private static bool OpenPort(IntVec3 cell, Map map)
        {
            if (!cell.InBounds(map) || cell.Impassable(map)) return false;
            foreach (Thing thing in cell.GetThingList(map))
            {
                if ((thing is Blueprint || thing is Frame) &&
                    thing.def.entityDefToBuild is ThingDef planned &&
                    planned.passability == Traversability.Impassable) return false;
            }
            return true;
        }

        public static AcceptanceReport Check(ThingDef def, IntVec3 center, Rot4 rotation, Map map,
            Thing thingToIgnore = null, Thing placingThing = null)
        {
            foreach (IntVec3 cell in GenAdj.OccupiedRect(center, rotation, def.size))
            {
                if (!HasWall(cell, map)) return "Minisplits_NeedsWall".Translate();
                foreach (Thing thing in cell.GetThingList(map))
                {
                    if (thing == thingToIgnore || thing == placingThing) continue;
                    ThingDef built = thing.def.entityDefToBuild as ThingDef ?? thing.def;
                    if (built.thingClass == typeof(Building_Minisplit))
                        return "Minisplits_AlreadyMounted".Translate();
                }
            }
            return CheckPorts(def, center, rotation, map);
        }

        public static AcceptanceReport CheckPorts(ThingDef def, IntVec3 center, Rot4 rotation, Map map)
        {
            // Placement previews may allocate; spawned units pass their cached cells instead.
            return CheckPorts(Ports(def, center, rotation, false), Ports(def, center, rotation, true), map);
        }

        public static AcceptanceReport CheckPorts(List<IntVec3> indoorPorts, List<IntVec3> exhaustPorts, Map map)
        {
            AcceptanceReport indoorResult = CheckSide(indoorPorts, map, out Room indoor);
            if (!indoorResult.Accepted) return indoorResult;
            AcceptanceReport exhaustResult = CheckSide(exhaustPorts, map, out Room outdoor);
            if (!exhaustResult.Accepted) return exhaustResult;
            if (indoor == outdoor) return "Minisplits_SameRoom".Translate();
            return true;
        }

        private static AcceptanceReport CheckSide(List<IntVec3> ports, Map map, out Room first)
        {
            // Cache coordinates only. Re-read rooms and obstructions as the map changes.
            first = null;
            for (int i = 0; i < ports.Count; i++)
            {
                IntVec3 port = ports[i];
                if (!OpenPort(port, map)) return "Minisplits_Blocked".Translate();
                Room room = port.GetRoom(map);
                if (room == null || (first != null && first != room))
                    return "Minisplits_SplitRoom".Translate();
                first = room;
            }
            return true;
        }
    }

    public class PlaceWorker_Minisplit : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot,
            Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            return MinisplitPlacement.Check((ThingDef)def, center, rot, map, thingToIgnore, thing);
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            GenDraw.DrawFieldEdges(MinisplitPlacement.Ports(def, center, rot, false), GenTemperature.ColorSpotCold);
            GenDraw.DrawFieldEdges(MinisplitPlacement.Ports(def, center, rot, true), GenTemperature.ColorSpotHot);
        }
    }
}
