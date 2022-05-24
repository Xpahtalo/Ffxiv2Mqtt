using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class MonkGaugeTracker : BaseGaugeTracker
    {
        private MonkGauge previousMonkGauge;

        public MonkGaugeTracker(MqttManager m) : base(m) { }

        public void Update(MNKGauge monkGauge)
        {
            // The crit chakra
            if (monkGauge.Chakra != previousMonkGauge.Chakra)
            {
                mqttManager.PublishMessage("JobGauge/MNK/Chakra", monkGauge.Chakra);
                previousMonkGauge.Chakra = monkGauge.Chakra;
            }

            // The Perfect Balance Chakra
            if (ToChakraType(monkGauge.BeastChakra[0]) != previousMonkGauge.BeastChakra1)
            {
                previousMonkGauge.BeastChakra1 = ToChakraType(monkGauge.BeastChakra[0]);
                mqttManager.PublishMessage("JobGauges/MNK/BeastChakra1", previousMonkGauge.BeastChakra1.ToString());
            }
            
            if (ToChakraType(monkGauge.BeastChakra[1]) != previousMonkGauge.BeastChakra2)
            {
                previousMonkGauge.BeastChakra2 = ToChakraType(monkGauge.BeastChakra[1]);
                mqttManager.PublishMessage("JobGauges/MNK/BeastChakra2", previousMonkGauge.BeastChakra2.ToString());
            }

            if (ToChakraType(monkGauge.BeastChakra[2]) != previousMonkGauge.BeastChakra3)
            {
                previousMonkGauge.BeastChakra3 = ToChakraType(monkGauge.BeastChakra[2]);
                mqttManager.PublishMessage("JobGauges/MNK/BeastChakra3", previousMonkGauge.BeastChakra3.ToString());
            }

            if (CheckCountDownTimer(previousMonkGauge.BlitzTimeRemaining, monkGauge.BlitzTimeRemaining, 1000))
            {
                previousMonkGauge.BlitzTimeRemaining = monkGauge.BlitzTimeRemaining;
                mqttManager.PublishMessage("JobGauges/MNK/BlitzTimer", previousMonkGauge.BlitzTimeRemaining);
            }
        }

        private BeastChakraType ToChakraType(BeastChakra beastChakra)
        {
            switch (beastChakra)
            {
                case BeastChakra.RAPTOR:
                    return BeastChakraType.Raptor;
                case BeastChakra.COEURL:
                    return BeastChakraType.Coeurl;
                case BeastChakra.OPOOPO:
                    return BeastChakraType.OpoOpo;
            }
            return BeastChakraType.None;
        }
    }
}
