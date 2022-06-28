using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class NinjaGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte Ninki { get => ninki; }
        public int HutonTimer { get => hutonTimer; }
        public byte ManualHutonCasts { get => hutonManualCasts; }
        
        private byte hutonManualCasts;
        private int hutonTimer;
        private byte ninki;

        private const uint NinjaId = 30;

        public NinjaGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/NIN";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != NinjaId)
                return;
            var gauge = DalamudServices.JobGauges.Get<NINGauge>();

            TestValue(gauge.HutonManualCasts, ref hutonManualCasts);
            TestCountDown(gauge.HutonTimer, ref hutonTimer, syncTimer);
            TestValue(gauge.Ninki, ref ninki);

            PublishIfNeeded();
        }
    }
}
