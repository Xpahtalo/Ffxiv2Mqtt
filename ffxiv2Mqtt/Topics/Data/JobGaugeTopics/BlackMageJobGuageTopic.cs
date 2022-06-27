using Dalamud.Logging;
using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class BlackMageJobGuageTopic : JobGaugeTopic, IUpdatable
    {
        public bool EnochianActive { get=> isEnochianActive;}
        public short EnochianTimeRemaining { get => enochianTimeRemaining; }
        public bool ParadoxActive { get=> isParadoxActive;}
        public byte AstralFireStacks { get => astralFireStacks; }
        public byte UmbralIceStacks { get => umbralIceStacks; }
        public short ElementTimeRemaining { get => elementTimeRemaining; }

        private bool isEnochianActive;
        private bool isParadoxActive;
        private byte astralFireStacks;
        private byte umbralIceStacks;
        private short elementTimeRemaining;
        private short enochianTimeRemaining;
        
        private const uint Thaumaturge = 7;
        private const uint BlackMageId = 25;

        public BlackMageJobGuageTopic(MqttManager m) : base(m) 
        {
            topic = "Player/JobGauge/BLM";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (!((localPlayer.ClassJob.Id == BlackMageId) || (localPlayer.ClassJob.Id == Thaumaturge)))
                return;
            var blmGauge = Dalamud.JobGauges.Get<BLMGauge>();

            TestValue(blmGauge.IsEnochianActive, ref isEnochianActive);
            TestValue(blmGauge.IsParadoxActive, ref isParadoxActive);
            TestValue(blmGauge.AstralFireStacks, ref astralFireStacks);
            TestValue(blmGauge.UmbralIceStacks, ref umbralIceStacks);
            TestCountDown(blmGauge.ElementTimeRemaining, ref elementTimeRemaining, syncTimer);
            TestCountDown(blmGauge.EnochianTimer, ref enochianTimeRemaining, syncTimer);

            PublishIfNeeded();
        }
    }
}
