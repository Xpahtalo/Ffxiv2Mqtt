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
            TestValue(reaperGauge.Soul, ref soul, "JobGauge/RPR/Soul");
            TestValue(reaperGauge.LemureShroud, ref lemureShroud, "JobGauge/RPR/LemureShroud");
            TestValue(reaperGauge.VoidShroud, ref voidShroud, "JobGauge/RPR/VoidShroud");
            TestCountDown(reaperGauge.EnshroudedTimeRemaining, ref enshroudedTimeRemaining, 1000, "JobGauge/RPR/EnshroudedTimer");
        }
    }
}
