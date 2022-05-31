using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class MachinistGaugeTracker : BaseGaugeTracker
    {
        private byte battery;
        private byte heat;
        private bool isOverheated;
        private bool isRobotActive;
        private byte lastSummonBatteryPower;
        private short overheatTimeRemaining;
        private short summonTimeRemaining;

        public MachinistGaugeTracker(MqttManager m) : base(m) { }

        public void Update(MCHGauge machinistGauge)
        {
            TestValue(machinistGauge.Battery, ref battery, "JobGauge/MCH/Battery");
            TestValue(machinistGauge.Heat, ref heat, "JobGauge/MCH/Heat");
            TestValue(machinistGauge.IsOverheated, ref isOverheated, "JobGauge/MCH/IsOverheated");
            TestValue(machinistGauge.IsRobotActive, ref isRobotActive, "JobGauge/MCH/IsRobotActive");
            TestValue(machinistGauge.LastSummonBatteryPower, ref lastSummonBatteryPower, "JobGauge/MCH/LastSummonBatteryPower");
            TestCountDown(machinistGauge.OverheatTimeRemaining, ref overheatTimeRemaining, 1000, "JobGauge/MCH/OverheatTimeRemaining");
            TestCountDown(machinistGauge.SummonTimeRemaining, ref summonTimeRemaining, 1000, "JobGauge/MCH/SummonTimeRemaining");
        }
    }
}
