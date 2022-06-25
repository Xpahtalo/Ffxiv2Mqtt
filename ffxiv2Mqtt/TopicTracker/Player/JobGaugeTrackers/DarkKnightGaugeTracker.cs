using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class DarkKnightGaugeTracker : BaseTopicTracker,IUpdatable
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

        public DarkKnightGaugeTracker(MqttManager m) : base(m) 
        {
            topic = "Player/JobGauge/DRK";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != DarkKnightId)
                return;
            var gauge = Dalamud.JobGauges.Get<DRKGauge>();

            TestValue(gauge.Blood, ref blood);
            TestCountDown(gauge.DarksideTimeRemaining, ref darksideTimeRemaining, 1000);
            TestValue(gauge.HasDarkArts, ref hasDarkArts);
            TestCountDown(gauge.ShadowTimeRemaining, ref shadowTimeRemaining, 1000);

            PublishIfNeeded();
        }
    }
}
