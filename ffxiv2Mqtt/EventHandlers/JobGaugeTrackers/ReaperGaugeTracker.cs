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
            mqttManager.TestValue(reaperGauge.Soul, ref soul, "JobGauge/RPR/Soul");
            mqttManager.TestValue(reaperGauge.LemureShroud, ref lemureShroud, "JobGauge/RPR/LemureShroud");
            mqttManager.TestValue(reaperGauge.VoidShroud, ref voidShroud, "JobGauge/RPR/VoidShroud");
            mqttManager.TestCountDown(reaperGauge.EnshroudedTimeRemaining, ref enshroudedTimeRemaining, 1000, "JobGauge/RPR/EnshroudedTimer");
        }
    }
}
