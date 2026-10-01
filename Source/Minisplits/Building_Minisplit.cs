using RimWorld;
using UnityEngine;
using Verse;

namespace Minisplits
{
    public class Building_Minisplit : Building_TempControl
    {
        private const float RareTickSeconds = 250f / 60f;
        private string state = "Minisplits_Idle";
        private MinisplitProperties Settings => (MinisplitProperties)compTempControl.Props;

        public override void TickRare()
        {
            // Run ordinary component maintenance, including breakdowns.
            base.TickRare();
            if (!Spawned) return;
            SetState("Minisplits_Idle", false);
            if (!compPowerTrader.PowerOn)
            {
                state = "Minisplits_Off";
                return;
            }
            foreach (IntVec3 cell in this.OccupiedRect())
            {
                if (!MinisplitPlacement.HasWall(cell, Map))
                {
                    state = "Minisplits_NeedsWall";
                    return;
                }
            }
            AcceptanceReport ports = MinisplitPlacement.CheckPorts(def, Position, Rotation, Map);
            if (!ports.Accepted)
            {
                state = null;
                blockedReason = ports.Reason;
                return;
            }
            blockedReason = null;
            var indoorPorts = MinisplitPlacement.Ports(def, Position, Rotation, false);
            var exhaustPorts = MinisplitPlacement.Ports(def, Position, Rotation, true);
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
