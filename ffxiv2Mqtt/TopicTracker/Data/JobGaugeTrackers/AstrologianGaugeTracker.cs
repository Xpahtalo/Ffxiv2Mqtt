using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class AstrologianGaugeTracker : BaseTopicTracker, IUpdatable
    {
        public CardType DrawnCard { get => drawnCard; }
        public CardType DrawnCrownCard { get => drawnCrownType; }
        public SealType[] Seals { get => seals; }
        private CardType drawnCard;
        private CardType drawnCrownType;
        private SealType[] seals;

        private const uint AstrologianId = 33;


        public AstrologianGaugeTracker(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/AST";
            seals = new SealType[3];
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != AstrologianId)
                return;
            var gauge = Dalamud.JobGauges.Get<ASTGauge>();


            TestValue(gauge.DrawnCard, ref drawnCard);
            TestValue(gauge.DrawnCrownCard, ref drawnCrownType);

            for (int i = 0; i < seals.Length; i++)
            {
                TestValue(gauge.Seals[i], ref seals[i]);
            }

            PublishIfNeeded();
        }
    }
}
