using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class SummonerGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte AetherflowStacks { get => aetherflowStacks; }
        public byte Attunement { get => attunement; }
        public ushort AttunementTimer { get => attunmentTimerRemaining; }
        public bool IsBahamutReady { get => isBahamutReady; }
        public bool IsPhoenixReady { get => isPhoenixReady; }
        public bool IsGarudaReady { get => isGarudaReady; }
        public bool IsIfritReady { get => isIfritReady; }
        public bool IsTitanReady { get => isTitanReady; }
        public bool IsGarudaAttuned { get => isGarudaAttuned; }
        public bool IsIfritAttuned { get => isIfritAttuned; }
        public bool IsTitanAttuned { get => isTitanAttuned; }
        public ushort SummonTimeRemaining { get => summonTimeRemaining; }


        private byte aetherflowStacks;
        private byte attunement;
        private ushort attunmentTimerRemaining;
        private bool isBahamutReady;
        private bool isPhoenixReady;
        private bool isGarudaReady;
        private bool isGarudaAttuned;
        private bool isIfritReady;
        private bool isIfritAttuned;
        private bool isTitanReady;
        private bool isTitanAttuned;
        private ushort summonTimeRemaining;


        public SummonerGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/SMN";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.Summoner)
                return;
            var gauge = DalamudServices.JobGauges.Get<SMNGauge>();

            TestValue(gauge.AetherflowStacks, ref aetherflowStacks);
            TestValue(gauge.Attunement, ref attunement);
            TestCountDown(gauge.AttunmentTimerRemaining, ref attunmentTimerRemaining, syncTimer);
            TestValue(gauge.IsBahamutReady, ref isBahamutReady);
            TestValue(gauge.IsPhoenixReady, ref isPhoenixReady);
            TestValue(gauge.IsGarudaReady, ref isGarudaReady);
            TestValue(gauge.IsGarudaAttuned, ref isGarudaAttuned);
            TestValue(gauge.IsIfritReady, ref isIfritReady);
            TestValue(gauge.IsIfritAttuned, ref isIfritAttuned);
            TestValue(gauge.IsTitanReady, ref isTitanReady);
            TestValue(gauge.IsTitanAttuned, ref isTitanAttuned);
            TestCountDown(gauge.SummonTimerRemaining, ref summonTimeRemaining, syncTimer);

            PublishIfNeeded();
        }
    }
}
