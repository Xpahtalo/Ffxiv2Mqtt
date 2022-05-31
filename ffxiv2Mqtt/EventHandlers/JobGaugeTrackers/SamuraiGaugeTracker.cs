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
            TestValue(samuraiGauge.Kenki, ref kenki, "JobGauge/SAM/Kenki");
            TestValue(samuraiGauge.MeditationStacks, ref meditationStacks, "JobGauge/SAM/MeditationStacks");
            TestValue(samuraiGauge.HasGetsu, ref hasGetsu, "JobGauge/SAM/Getsu");
            TestValue(samuraiGauge.HasKa, ref hasKa, "JobGauge/SAM/Ka");
            TestValue(samuraiGauge.HasSetsu, ref hasSetsu, "JobGauge/SAM/Setsu");
            TestValue(samuraiGauge.Kaeshi, ref kaeshi, "JobGauge/SAM/Kaeshi");
        }
    }
}
