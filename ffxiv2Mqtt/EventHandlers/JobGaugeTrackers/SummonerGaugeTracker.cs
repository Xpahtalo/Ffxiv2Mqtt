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
            mqttManager.TestValue(summonerGauge.AetherflowStacks, ref aetherflowStacks, "JobGauge/SMN/AetherflowStacks");
            mqttManager.TestValue(summonerGauge.Attunement, ref attunement, "JobGauge/SMN/Attunement");
            mqttManager.TestCountDown(summonerGauge.AttunmentTimerRemaining, ref attunmentTimerRemaining, 1000, "JobGauge/SMN/AttunementTimer");
            mqttManager.TestValue(summonerGauge.IsBahamutReady, ref isBahamutReady, "JobGauge/SMN/BahamutReady");
            mqttManager.TestValue(summonerGauge.IsPhoenixReady, ref isPhoenixReady, "JobGauge/SMN/PhoenixReady");
            mqttManager.TestValue(summonerGauge.IsGarudaReady, ref isGarudaReady, "JobGauge/SMN/GarudaReady");
            mqttManager.TestValue(summonerGauge.IsGarudaAttuned, ref isGarudaAttuned, "JobGauge/SMN/GarudaAttuned");
            mqttManager.TestValue(summonerGauge.IsIfritReady, ref isIfritReady, "JobGauge/SMN/IfritReady");
            mqttManager.TestValue(summonerGauge.IsIfritAttuned, ref isIfritAttuned, "JobGauge/SMN/IfritAttuned");
            mqttManager.TestValue(summonerGauge.IsTitanReady, ref isTitanReady, "JobGauge/SMN/TitanReady");
            mqttManager.TestValue(summonerGauge.IsTitanAttuned, ref isTitanAttuned, "JobGauge/SMN/TitanAttuned");
            mqttManager.TestCountDown(summonerGauge.SummonTimerRemaining, ref summonTimeRemaining, 1000, "JobGauge/SMN/SummonTimeRemaining");
        }
    }
}
