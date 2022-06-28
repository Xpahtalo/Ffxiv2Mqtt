using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class DarkKnightGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte Blood { get => blood; }
        public bool DarkArts { get=> hasDarkArts; }
        public ushort DarksideTimeRemaining { get=>darksideTimeRemaining; }
        public ushort ShadowTimeRemaining { get=>darksideTimeRemaining; }

        private byte blood;
        private bool hasDarkArts;
        private ushort darksideTimeRemaining;
        private ushort shadowTimeRemaining;

        private const uint DarkKnightId = 32;

        public DarkKnightGaugeTopic(MqttManager m) : base(m) 
        {
            topic = "Player/JobGauge/DRK";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != DarkKnightId)
                return;
            var gauge = DalamudServices.JobGauges.Get<DRKGauge>();

            TestValue(gauge.Blood, ref blood);
            TestCountDown(gauge.DarksideTimeRemaining, ref darksideTimeRemaining, syncTimer);
            TestValue(gauge.HasDarkArts, ref hasDarkArts);
            TestCountDown(gauge.ShadowTimeRemaining, ref shadowTimeRemaining, syncTimer);

            PublishIfNeeded();
        }
    }
}
