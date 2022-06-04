using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class SummonerGaugeTracker : BaseGaugeTracker
    {
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

        public SummonerGaugeTracker(MqttManager m) : base(m) { }
        
        public void Update(SMNGauge summonerGauge)
        {
            TestValue(summonerGauge.AetherflowStacks, ref aetherflowStacks, "JobGauge/SMN/AetherflowStacks");
            TestValue(summonerGauge.Attunement, ref attunement, "JobGauge/SMN/Attunement");
            TestCountDown(summonerGauge.AttunmentTimerRemaining, ref attunmentTimerRemaining, 1000, "JobGauge/SMN/AttunementTimer");
            TestValue(summonerGauge.IsBahamutReady, ref isBahamutReady, "JobGauge/SMN/BahamutReady");
            TestValue(summonerGauge.IsPhoenixReady, ref isPhoenixReady, "JobGauge/SMN/PhoenixReady");
            TestValue(summonerGauge.IsGarudaReady, ref isGarudaReady, "JobGauge/SMN/GarudaReady");
            TestValue(summonerGauge.IsGarudaAttuned, ref isGarudaAttuned, "JobGauge/SMN/GarudaAttuned");
            TestValue(summonerGauge.IsIfritReady, ref isIfritReady, "JobGauge/SMN/IfritReady");
            TestValue(summonerGauge.IsIfritAttuned, ref isIfritAttuned, "JobGauge/SMN/IfritAttuned");
            TestValue(summonerGauge.IsTitanReady, ref isTitanReady, "JobGauge/SMN/TitanReady");
            TestValue(summonerGauge.IsTitanAttuned, ref isTitanAttuned, "JobGauge/SMN/TitanAttuned");
            TestCountDown(summonerGauge.SummonTimerRemaining, ref summonTimeRemaining, 1000, "JobGauge/SMN/SummonTimeRemaining");
        }
    }
}
