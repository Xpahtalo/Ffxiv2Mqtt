using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class SageGaugeTracker : BaseGaugeTracker
    {
        private byte addersgall;
        private short addersgallTimer;
        private byte addersting;
        private bool eukrasia;

        public SageGaugeTracker(MqttManager m) : base(m) { }

        public void Update(SGEGauge sageGauge)
        {
            TestValue(sageGauge.Addersgall, ref addersgall, "JobGauge/SGE/Addersgall");
            TestCountUp(sageGauge.AddersgallTimer, ref addersgallTimer, 1000, "JobGauge/SGE/AddersgallTimer");
            TestValue(sageGauge.Addersting, ref addersting, "JobGauge/SGE/Addersting");
            TestValue(sageGauge.Eukrasia, ref eukrasia, "JobGauge/SGE/Eukrasia");
        }
    }
}
