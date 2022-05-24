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
            if (machinistGauge.Battery != battery)
            {
                mqttManager.PublishMessage("JobGauge/MCH/Battery", machinistGauge.Battery);
                battery = machinistGauge.Battery;
            }

            if (machinistGauge.Heat != heat)
            {
                mqttManager.PublishMessage("JobGauge/MCH/Heat", machinistGauge.Heat);
                heat = machinistGauge.Heat;
            }

            if (machinistGauge.IsOverheated != isOverheated)
            {
                mqttManager.PublishMessage("JobGauge/MCH/IsOverheated", machinistGauge.IsOverheated);
                isOverheated = machinistGauge.IsOverheated;
            }

            if (machinistGauge.IsRobotActive != isRobotActive)
            {
                mqttManager.PublishMessage("JobGauge/MCH/IsRobotActive", machinistGauge.IsRobotActive);
                isRobotActive = machinistGauge.IsRobotActive;
            }

            if (machinistGauge.LastSummonBatteryPower != lastSummonBatteryPower)
            {
                mqttManager.PublishMessage("JobGauge/MCH/LastSummonBatteryPower", machinistGauge.LastSummonBatteryPower);
                lastSummonBatteryPower = machinistGauge.LastSummonBatteryPower;
            }

            if (CheckCountDownTimer(overheatTimeRemaining, machinistGauge.OverheatTimeRemaining, 1000))
            {
                mqttManager.PublishMessage("JobGauge/MCH/OverheatTimeRemaining", machinistGauge.OverheatTimeRemaining);
                overheatTimeRemaining = machinistGauge.OverheatTimeRemaining;
            }

            if (CheckCountDownTimer(summonTimeRemaining, machinistGauge.SummonTimeRemaining, 1000))
            {
                mqttManager.PublishMessage("JobGauge/MCH/SummonTimeRemaining", machinistGauge.SummonTimeRemaining);
                summonTimeRemaining = machinistGauge.SummonTimeRemaining;
            }
        }
    }
}
