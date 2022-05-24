using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class BlackMageGuageTracker : BaseGaugeTracker
    {
        private bool enochianActive;
        private bool paradoxActive;
        private byte astralFireStacks;
        private byte umbralIceStacks;
        private short elementTimeRemaining;
        private short enochianTimeRemaining;


        public BlackMageGuageTracker(MqttManager m) : base(m) { }

        public void Update(BLMGauge blackMageGuage)
        {
            if (blackMageGuage.IsEnochianActive != enochianActive)
            {
                mqttManager.PublishMessage("JobGauge/BLM/EnochianActive", blackMageGuage.IsEnochianActive);
                enochianActive = blackMageGuage.IsEnochianActive;
            }

            if (blackMageGuage.IsParadoxActive != paradoxActive)
            {
                mqttManager.PublishMessage("JobGauge/BLM/ParadoxActive", blackMageGuage.IsParadoxActive);
                paradoxActive = blackMageGuage.IsParadoxActive;
            }
            
            if (blackMageGuage.AstralFireStacks != astralFireStacks)
            {
                mqttManager.PublishMessage("JobGauge/BLM/AstralFireStacks", blackMageGuage.AstralFireStacks);
                astralFireStacks = blackMageGuage.AstralFireStacks;
            }

            if (blackMageGuage.UmbralIceStacks != umbralIceStacks)
            {
                mqttManager.PublishMessage("JobGauge/BLM/UmbralIceStacks", blackMageGuage.UmbralIceStacks);
                umbralIceStacks = blackMageGuage.UmbralIceStacks;
            }

            if (CheckCountDownTimer(elementTimeRemaining, blackMageGuage.ElementTimeRemaining, 1000))
            {
                mqttManager.PublishMessage("JobGauge/BLM/ElementTimer", blackMageGuage.ElementTimeRemaining);
                elementTimeRemaining = blackMageGuage.ElementTimeRemaining;
            }

            if (CheckCountDownTimer(enochianTimeRemaining, blackMageGuage.EnochianTimer, 1000))
            {
                mqttManager.PublishMessage("JobGauge/BLM/EnochianTimer", blackMageGuage.EnochianTimer);
                enochianTimeRemaining = blackMageGuage.EnochianTimer;
            }
        }
    }
}
