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
            mqttManager.TestValue(machinistGauge.Battery, ref battery, "JobGauge/MCH/Battery");
            mqttManager.TestValue(machinistGauge.Heat, ref heat, "JobGauge/MCH/Heat");
            mqttManager.TestValue(machinistGauge.IsOverheated, ref isOverheated, "JobGauge/MCH/IsOverheated");
            mqttManager.TestValue(machinistGauge.IsRobotActive, ref isRobotActive, "JobGauge/MCH/IsRobotActive");
            mqttManager.TestValue(machinistGauge.LastSummonBatteryPower, ref lastSummonBatteryPower, "JobGauge/MCH/LastSummonBatteryPower");
            mqttManager.TestCountDown(machinistGauge.OverheatTimeRemaining, ref overheatTimeRemaining, 1000, "JobGauge/MCH/OverheatTimeRemaining");
            mqttManager.TestCountDown(machinistGauge.SummonTimeRemaining, ref summonTimeRemaining, 1000, "JobGauge/MCH/SummonTimeRemaining");
        }
    }
}
