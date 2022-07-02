using Ffxiv2Mqtt.Enums;
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
            if ((Job)localPlayer.ClassJob.Id != Job.Ninja)
                return;
            
            var gauge = DalamudServices.JobGauges.Get<NINGauge>();
            TestValue(gauge.HutonManualCasts, ref hutonManualCasts);
            TestCountDown(gauge.HutonTimer, ref hutonTimer, syncTimer);
            TestValue(gauge.Ninki, ref ninki);

            PublishIfNeeded();
        }
    }
}
