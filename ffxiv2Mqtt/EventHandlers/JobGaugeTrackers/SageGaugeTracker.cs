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
            if (sageGauge.Addersgall != addersgall)
            {
                mqttManager.PublishMessage("JobGauge/SGE/Addersgall", sageGauge.Addersgall);
                addersgall = sageGauge.Addersgall;
            }

            if (CheckCountUpTimer(addersgallTimer, sageGauge.AddersgallTimer, 1000))
            {
                mqttManager.PublishMessage("JobGauge/SGE/AddersgallTimer", sageGauge.AddersgallTimer);
                addersgallTimer = sageGauge.AddersgallTimer;
            }

            if (sageGauge.Addersting != addersting)
            {
                mqttManager.PublishMessage("JobGauge/SGE/Addersting", sageGauge.Addersting);
                addersting = sageGauge.Addersting;
            }

            if (sageGauge.Eukrasia != eukrasia)
            {
                mqttManager.PublishMessage("JobGauge/SGE/Eukrasia", sageGauge.Eukrasia);
                eukrasia = sageGauge.Eukrasia;
            }
        }
    }
}
