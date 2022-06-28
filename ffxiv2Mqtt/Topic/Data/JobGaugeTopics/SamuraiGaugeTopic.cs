using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class SamuraiGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte Kenki { get => kenki; }
        public byte MeditationStacks { get => meditationStacks; }
        public bool Getsu { get => hasGetsu; }
        public bool Ka { get => hasKa; }
        public bool Setsu { get => hasSetsu; }
        public Kaeshi Kaeshi { get => kaeshi; }

        private byte kenki;
        private byte meditationStacks;
        private bool hasGetsu;
        private bool hasKa;
        private bool hasSetsu;
        private Kaeshi kaeshi;

        private const uint SamuraiId = 34;

        public SamuraiGaugeTopic(MqttManager m) : base(m) {
            topic = "Player/JobGauge/SAM";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != SamuraiId)
                return;
            var gauge = DalamudServices.JobGauges.Get<SAMGauge>();

            TestValue(gauge.Kenki, ref kenki);
            TestValue(gauge.MeditationStacks, ref meditationStacks);
            TestValue(gauge.HasGetsu, ref hasGetsu);
            TestValue(gauge.HasKa, ref hasKa);
            TestValue(gauge.HasSetsu, ref hasSetsu);
            TestValue(gauge.Kaeshi, ref kaeshi);

            PublishIfNeeded();
        }
    }
}
