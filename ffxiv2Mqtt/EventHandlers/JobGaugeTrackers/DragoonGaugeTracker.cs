using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class DragoonGaugeTracker : BaseGaugeTracker
    {
        private byte eyeCount;
        private byte firstmindsFocusCount;
        private bool isLotdActive;
        private short lotdTimer;

        public DragoonGaugeTracker(MqttManager m) : base(m) { }

        public void Update(DRGGauge dragoonGauge)
        {
            mqttManager.TestValue(dragoonGauge.EyeCount, ref eyeCount, "JobGauge/DRG/Eye");
            mqttManager.TestValue(dragoonGauge.FirstmindsFocusCount, ref firstmindsFocusCount, "JobGauge/DRG/FirstmindsFocus");
            mqttManager.TestValue(dragoonGauge.IsLOTDActive, ref isLotdActive, "JobGauge/DRG/IsLOTDActive");
            mqttManager.TestCountDown(dragoonGauge.LOTDTimer, ref lotdTimer, 1000, "JobGauge/DRG/LifeOfTheDragonTimeRemaining");
        }
    }
}
