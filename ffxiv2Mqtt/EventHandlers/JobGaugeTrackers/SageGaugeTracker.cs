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
            mqttManager.TestValue(sageGauge.Addersgall, ref addersgall, "JobGauge/SGE/Addersgall");
            mqttManager.TestCountUp(sageGauge.AddersgallTimer, ref addersgallTimer, 1000, "JobGauge/SGE/AddersgallTimer");
            mqttManager.TestValue(sageGauge.Addersting, ref addersting, "JobGauge/SGE/Addersting");
            mqttManager.TestValue(sageGauge.Eukrasia, ref eukrasia, "JobGauge/SGE/Eukrasia");
        }
    }
}
