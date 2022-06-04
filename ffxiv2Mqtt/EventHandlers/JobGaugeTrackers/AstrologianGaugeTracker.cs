using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class AstrologianGaugeTracker : BaseGaugeTracker
    {
        private CardType drawnCard;
        private CardType drawnCrownType;
        private SealType[] seals;
        

        public AstrologianGaugeTracker(MqttManager m) : base(m) 
        {
            seals = new SealType[3];
        }

        public void Update(ASTGauge astrologianGauge)
        {
            mqttManager.TestValue(astrologianGauge.DrawnCard, ref drawnCard, "JobGauge/AST/DrawnCard");
            mqttManager.TestValue(astrologianGauge.DrawnCrownCard, ref drawnCrownType, "JobGauge/AST/DrawnCrownType");
            
            for (int i = 0; i < seals.Length; i++)
            {
                mqttManager.TestValue(astrologianGauge.Seals[i], ref seals[i], string.Format("JobGauge/AST/Seal{0}", i + 1));
            }
        }
    }
}
