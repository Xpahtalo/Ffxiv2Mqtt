using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class BlackMageGuageTracker : BaseGaugeTracker
    {
        private bool isEnochianActive;
        private bool isParadoxActive;
        private byte astralFireStacks;
        private byte umbralIceStacks;
        private short elementTimeRemaining;
        private short enochianTimeRemaining;


        public BlackMageGuageTracker(MqttManager m) : base(m) { }

        public void Update(BLMGauge blackMageGuage)
        {
            mqttManager.TestValue(blackMageGuage.IsEnochianActive, ref isEnochianActive, "JobGauge/BLM/EnochianActive");
            mqttManager.TestValue(blackMageGuage.IsParadoxActive, ref isParadoxActive, "JobGauge/BLM/ParadoxActive");
            mqttManager.TestValue(blackMageGuage.AstralFireStacks, ref astralFireStacks, "JobGauge/BLM/AstralFireStacks");
            mqttManager.TestValue(blackMageGuage.UmbralIceStacks, ref umbralIceStacks, "JobGauge/BLM/UmbralIceStacks");
            mqttManager.TestCountDown(blackMageGuage.ElementTimeRemaining, ref elementTimeRemaining, 1000, "JobGauge/BLM/ElementTimer");
            mqttManager.TestCountDown(blackMageGuage.EnochianTimer, ref enochianTimeRemaining, 1000, "JobGauge/BLM/EnochianTimer");
        }
    }
}
