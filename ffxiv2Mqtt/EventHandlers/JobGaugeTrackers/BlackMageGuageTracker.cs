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
            TestValue(blackMageGuage.IsEnochianActive, ref isEnochianActive, "JobGauge/BLM/EnochianActive");
            TestValue(blackMageGuage.IsParadoxActive, ref isParadoxActive, "JobGauge/BLM/ParadoxActive");
            TestValue(blackMageGuage.AstralFireStacks, ref astralFireStacks, "JobGauge/BLM/AstralFireStacks");
            TestValue(blackMageGuage.UmbralIceStacks, ref umbralIceStacks, "JobGauge/BLM/UmbralIceStacks");
            TestCountDown(blackMageGuage.ElementTimeRemaining, ref elementTimeRemaining, 1000, "JobGauge/BLM/ElementTimer");
            TestCountDown(blackMageGuage.EnochianTimer, ref enochianTimeRemaining, 1000, "JobGauge/BLM/EnochianTimer");
        }
    }
}
