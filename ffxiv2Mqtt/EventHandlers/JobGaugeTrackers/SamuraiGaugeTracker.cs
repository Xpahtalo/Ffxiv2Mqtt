using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class SamuraiGaugeTracker : BaseGaugeTracker
    {
        private byte kenki;
        private byte meditationStacks;
        private bool hasGetsu;
        private bool hasKa;
        private bool hasSetsu;
        private Kaeshi kaeshi;

        public SamuraiGaugeTracker(MqttManager m) : base(m) { }

        public void Update(SAMGauge samuraiGauge)
        {
            mqttManager.TestValue(samuraiGauge.Kenki, ref kenki, "JobGauge/SAM/Kenki");
            mqttManager.TestValue(samuraiGauge.MeditationStacks, ref meditationStacks, "JobGauge/SAM/MeditationStacks");
            mqttManager.TestValue(samuraiGauge.HasGetsu, ref hasGetsu, "JobGauge/SAM/Getsu");
            mqttManager.TestValue(samuraiGauge.HasKa, ref hasKa, "JobGauge/SAM/Ka");
            mqttManager.TestValue(samuraiGauge.HasSetsu, ref hasSetsu, "JobGauge/SAM/Setsu");
            mqttManager.TestValue(samuraiGauge.Kaeshi, ref kaeshi, "JobGauge/SAM/Kaeshi");
        }
    }
}
