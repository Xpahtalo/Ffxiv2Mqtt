using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class NinjaGaugeTracker : BaseGaugeTracker
    {
        private byte hutonManualCasts;
        private int hutonTimer;
        public byte ninki;

        public NinjaGaugeTracker(MqttManager m) : base(m) { }

        public void Update(NINGauge ninjaGauge)
        {
            mqttManager.TestValue(ninjaGauge.HutonManualCasts, ref hutonManualCasts, "JobGauge/NIN/HutonManualCasts");
            mqttManager.TestCountDown(ninjaGauge.HutonTimer, ref hutonTimer, 1000, "JobGauge/NIN/HutonTimer");
            mqttManager.TestValue(ninjaGauge.Ninki, ref ninki, "JobGauge/NIN/Ninki");
        }
    }
}
