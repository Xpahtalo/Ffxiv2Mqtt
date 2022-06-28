using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class AstrologianGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public CardType DrawnCard { get => drawnCard; }
        public CardType DrawnCrownCard { get => drawnCrownType; }
        public SealType[] Seals { get => seals; }
        
        private CardType drawnCard;
        private CardType drawnCrownType;
        private SealType[] seals;


        public AstrologianGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/AST";
            seals = new SealType[3];
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.Astrologian)
                return;
            
            var gauge = DalamudServices.JobGauges.Get<ASTGauge>();
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
