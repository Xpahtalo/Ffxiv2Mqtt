using Ffxiv2Mqtt.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class ReaperGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte Soul { get => soul; }
        public byte LemureShroud { get => lemureShroud; }
        public byte VoidShroud { get => voidShroud; }
        public ushort EnshroudedTimer { get => enshroudedTimeRemaining; }

        private byte soul;
        private byte lemureShroud;
        private byte voidShroud;
        private ushort enshroudedTimeRemaining;


        public ReaperGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/RPR";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.Reaper)
                return;
            
            var gauge = DalamudServices.JobGauges.Get<RPRGauge>();
            TestValue(gauge.Soul, ref soul);
            TestValue(gauge.LemureShroud, ref lemureShroud);
            TestValue(gauge.VoidShroud, ref voidShroud);
            TestCountDown(gauge.EnshroudedTimeRemaining, ref enshroudedTimeRemaining, syncTimer);

            PublishIfNeeded();
        }
    }
}
