using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Minisplits
{
    public class Building_Minisplit : Building_TempControl
    {
        private const float RareTickSeconds = 250f / 60f;
        private readonly List<IntVec3> indoorPorts = new List<IntVec3>(2);
        private readonly List<IntVec3> exhaustPorts = new List<IntVec3>(2);
        private readonly List<IntVec3> wallCells = new List<IntVec3>(2);
        private IntVec3 cachedPosition;
        private Rot4 cachedRotation;
        private bool cellsCached;
        private string state = "Minisplits_Idle";
        private MinisplitProperties Settings => (MinisplitProperties)compTempControl.Props;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            cellsCached = false;
            EnsureCellCache();
        }

        private void EnsureCellCache()
        {
            if (cellsCached && cachedPosition == Position && cachedRotation == Rotation) return;
            wallCells.Clear();
            indoorPorts.Clear();
            exhaustPorts.Clear();
            IntVec3 indoorOffset = IntVec3.South.RotatedBy(Rotation);
            IntVec3 exhaustOffset = IntVec3.North.RotatedBy(Rotation);
            foreach (IntVec3 cell in this.OccupiedRect())
            {
                wallCells.Add(cell);
                indoorPorts.Add(cell + indoorOffset);
                exhaustPorts.Add(cell + exhaustOffset);
            }
            cachedPosition = Position;
            cachedRotation = Rotation;
            cellsCached = true;
        }

        public override void TickRare()
        {
            // Run ordinary component maintenance, including breakdowns.
            base.TickRare();
            if (!Spawned) return;
            EnsureCellCache();
            SetState("Minisplits_Idle", false);
            if (!compPowerTrader.PowerOn)
            {
                state = "Minisplits_Off";
                return;
            }
            foreach (IntVec3 cell in wallCells)
            {
                if (!MinisplitPlacement.HasWall(cell, Map))
                {
                    state = "Minisplits_NeedsWall";
                    return;
                }
            }
            AcceptanceReport ports = MinisplitPlacement.CheckPorts(indoorPorts, exhaustPorts, Map);
            if (!ports.Accepted)
            {
                state = null;
                blockedReason = ports.Reason;
                return;
            }
            blockedReason = null;
            IntVec3 indoorCell = indoorPorts[0];
            Room room = indoorCell.GetRoom(Map);
            float current = room.Temperature;
            float target = compTempControl.TargetTemperature;
            float energy;
            bool cooling = current > target + Settings.deadband;

            if (cooling)
            {
                float exhaustTemperature = exhaustPorts[0].GetTemperature(Map);
                // Vanilla cooler efficiency curve, including the hot exhaust penalty.
                float burden = Mathf.Max(exhaustTemperature - current, exhaustTemperature - 40f);
                float efficiency = Mathf.Max(0f, 1f - burden / 130f);
                energy = -Mathf.Abs(Settings.energyPerSecond) * efficiency * RareTickSeconds;
            }
            else if (current < target - Settings.deadband)
            {
                // Vanilla heater output falls off above 20 C and reaches zero at 120 C.
                float efficiency = current <= 20f ? 1f : Mathf.Clamp01((120f - current) / 100f);
                energy = Settings.heatingEnergyPerSecond * efficiency * RareTickSeconds;
            }
            else return;

            float change = GenTemperature.ControlTemperatureTempChange(indoorCell, Map, energy, target);
            if (Mathf.Approximately(change, 0f)) return;
            room.Temperature += change;
            if (cooling)
            {
                // Same 1.25x exhaust model as vanilla. Large unit distributes its output.
                float heatPerPort = -energy * 1.25f / exhaustPorts.Count;
                foreach (IntVec3 port in exhaustPorts) GenTemperature.PushHeat(port, Map, heatPerPort);
            }
            SetState(cooling ? "Minisplits_Cooling" : "Minisplits_Heating", true);
        }

        private string blockedReason;

        private void SetState(string key, bool active)
        {
            state = key;
            blockedReason = null;
            compTempControl.operatingAtHighPower = active;
            compPowerTrader.PowerOutput = -compPowerTrader.Props.PowerConsumption *
                (active ? 1f : Settings.lowPowerConsumptionFactor);
        }

        public override string GetInspectString()
        {
            return base.GetInspectString() + "\n" + "Minisplits_Mode".Translate() + ": " +
                (state == null ? blockedReason : (string)state.Translate());
        }
    }
}
