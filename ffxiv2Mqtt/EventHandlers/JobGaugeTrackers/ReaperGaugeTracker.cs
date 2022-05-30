using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class ReaperGaugeTracker : BaseGaugeTracker
    {
        private byte soul;
        private byte lemureShroud;
        private byte voidShroud;
        private ushort enshroudedTimeRemaining;


        public ReaperGaugeTracker(MqttManager m) : base(m) { }

        public void Update(RPRGauge reaperGauge)
        {
            if (reaperGauge.Soul != soul)
            {
                mqttManager.PublishMessage("JobGauge/RPR/Soul", reaperGauge.Soul);
                soul = reaperGauge.Soul;
            }

            if (reaperGauge.LemureShroud != lemureShroud)
            {
                mqttManager.PublishMessage("JobGauge/RPR/LemureShroud", reaperGauge.LemureShroud);
                lemureShroud = reaperGauge.LemureShroud;
            }

            if (reaperGauge.VoidShroud != voidShroud)
            {
                mqttManager.PublishMessage("JobGauge/RPR/VoidShroud", reaperGauge.VoidShroud);
                voidShroud = reaperGauge.VoidShroud;
            }

            if (CheckCountDownTimer(enshroudedTimeRemaining, reaperGauge.EnshroudedTimeRemaining, 1000))
            {
                mqttManager.PublishMessage("JobGauge/RPR/EnshroudedTimer", reaperGauge.EnshroudedTimeRemaining);
                enshroudedTimeRemaining = reaperGauge.EnshroudedTimeRemaining;
            }
        }
    }
}
