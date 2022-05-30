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
            if (astrologianGauge.DrawnCard != drawnCard)
            {
                mqttManager.PublishMessage("JobGauge/AST/DrawnCard", astrologianGauge.DrawnCard.ToString());
                drawnCard = astrologianGauge.DrawnCard;
            }

            if (astrologianGauge.DrawnCrownCard != drawnCrownType)
            {
                mqttManager.PublishMessage("JobGauge/AST/DrawnCrownType", astrologianGauge.DrawnCrownCard.ToString());
                drawnCrownType = astrologianGauge.DrawnCrownCard;
            }

            for (int i = 0; i < seals.Length; i++)
            {
                if (astrologianGauge.Seals[i] != seals[i])
                {
                    mqttManager.PublishMessage(string.Format("JobGauge/AST/Seal{0}", i+1), astrologianGauge.Seals[i].ToString());
                    seals[i] = astrologianGauge.Seals[i];
                }
            }
        }
    }
}
