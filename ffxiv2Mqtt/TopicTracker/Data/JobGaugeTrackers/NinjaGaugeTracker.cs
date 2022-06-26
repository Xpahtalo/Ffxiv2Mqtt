using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class NinjaGaugeTracker : BaseGaugeTracker, IUpdatable
    {
        public byte Ninki { get => ninki; }
        public int HutonTimer { get => hutonTimer; }
        public byte ManualHutonCasts { get => hutonManualCasts; }
        
        private byte hutonManualCasts;
        private int hutonTimer;
        private byte ninki;

        private const uint NinjaId = 30;

        public NinjaGaugeTracker(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/NIN";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != NinjaId)
                return;
            var gauge = Dalamud.JobGauges.Get<NINGauge>();

            TestValue(gauge.HutonManualCasts, ref hutonManualCasts);
            TestCountDown(gauge.HutonTimer, ref hutonTimer, syncTimer);
            TestValue(gauge.Ninki, ref ninki);

            PublishIfNeeded();
        }
    }
}
