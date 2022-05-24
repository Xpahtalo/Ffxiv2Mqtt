using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class ReaperGaugeTracker : BaseGaugeTracker
    {
        private ReaperGauge previousReaperGauge;

        public ReaperGaugeTracker(MqttManager m) : base(m) { }

        public void Update(RPRGauge reaperGauge)
        {
            if (reaperGauge.Soul != previousReaperGauge.Soul)
            {
                mqttManager.PublishMessage("JobGauge/RPR/Soul", reaperGauge.Soul);
                previousReaperGauge.Soul = reaperGauge.Soul;
            }

            if (reaperGauge.LemureShroud != previousReaperGauge.LemureShroud)
            {
                mqttManager.PublishMessage("JobGauge/RPR/LemureShroud", reaperGauge.LemureShroud);
                previousReaperGauge.LemureShroud = reaperGauge.LemureShroud;
            }

            if (reaperGauge.VoidShroud != previousReaperGauge.VoidShroud)
            {
                mqttManager.PublishMessage("JobGauge/RPR/VoidShroud", reaperGauge.VoidShroud);
                previousReaperGauge.VoidShroud = reaperGauge.VoidShroud;
            }

            if (CheckCountDownTimer(previousReaperGauge.EnshroudedTimeRemaining, reaperGauge.EnshroudedTimeRemaining, 1000))
            {
                mqttManager.PublishMessage("JobGauge/RPR/EnshroudedTimer", reaperGauge.EnshroudedTimeRemaining);
                previousReaperGauge.EnshroudedTimeRemaining = reaperGauge.EnshroudedTimeRemaining;
            }
        }
    }
}
