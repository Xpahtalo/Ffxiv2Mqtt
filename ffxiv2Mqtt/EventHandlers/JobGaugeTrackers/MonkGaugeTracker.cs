using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class MonkGaugeTracker : BaseGaugeTracker
    {
        private byte chakra;
        private BeastChakra[] beastChakra;
        private ushort blitzTimeRemaining;


        public MonkGaugeTracker(MqttManager m) : base(m)
        {
            beastChakra = new BeastChakra[3];
        }

        public void Update(MNKGauge monkGauge)
        {
            // The crit chakra
            if (monkGauge.Chakra != chakra)
            {
                mqttManager.PublishMessage("JobGauge/MNK/Chakra", monkGauge.Chakra);
                chakra = monkGauge.Chakra;
            }

            // Perfect Balance Chakra
            for (var i = 0; i < 3; i++)
            {
                if (monkGauge.BeastChakra[i] != beastChakra[i])
                {
                    mqttManager.PublishMessage(string.Format("JobGauges/MNK/BeastChakra{0}", i + 1), monkGauge.BeastChakra[i].ToString());
                    beastChakra[i] = monkGauge.BeastChakra[i];
                }
            }


            if (CheckCountDownTimer(blitzTimeRemaining, monkGauge.BlitzTimeRemaining, 1000))
            {
                mqttManager.PublishMessage("JobGauges/MNK/BlitzTimer", monkGauge.BlitzTimeRemaining);
                blitzTimeRemaining = monkGauge.BlitzTimeRemaining;
            }
        }
    }
}
