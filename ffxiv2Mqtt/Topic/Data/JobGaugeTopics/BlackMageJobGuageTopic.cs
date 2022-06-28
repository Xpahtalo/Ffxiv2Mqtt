using Dalamud.Logging;
using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
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
        

        public BlackMageJobGuageTopic(MqttManager m) : base(m) 
        {
            topic = "Player/JobGauge/BLM";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (!((Job)localPlayer.ClassJob.Id == Job.BlackMage || (Job)localPlayer.ClassJob.Id == Job.Thaumaturge))
                return;
            var blmGauge = DalamudServices.JobGauges.Get<BLMGauge>();

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
