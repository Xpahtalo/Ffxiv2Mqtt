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
            if (dragoonGauge.EyeCount != eyeCount)
            {
                mqttManager.PublishMessage("JobGauge/DRG/Eye", dragoonGauge.EyeCount);
                eyeCount = dragoonGauge.EyeCount;
            }

            if (dragoonGauge.FirstmindsFocusCount != firstmindsFocusCount)
            {
                mqttManager.PublishMessage("JobGauge/DRG/FirstmindsFocus", dragoonGauge.FirstmindsFocusCount);
                firstmindsFocusCount  = dragoonGauge.FirstmindsFocusCount;
            }

            if (dragoonGauge.IsLOTDActive != isLotdActive)
            {
                mqttManager.PublishMessage("JobGauge/DRG/Life", dragoonGauge.IsLOTDActive);
                isLotdActive = dragoonGauge.IsLOTDActive;
            }

            if (CheckCountDownTimer(lotdTimer, dragoonGauge.LOTDTimer, 1000))
            {
                mqttManager.PublishMessage("JobGauges/DRG/LifeTimer", dragoonGauge.LOTDTimer);
                lotdTimer = dragoonGauge.LOTDTimer;
            }
        }
    }
}
