using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class ReaperGaugeTracker : BaseTopicTracker, IUpdatable
    {
        public byte Soul { get => soul; }
        public byte LemureShroud { get => lemureShroud; }
        public byte VoidShroud { get => voidShroud; }
        public ushort EnshroudedTimer { get => enshroudedTimeRemaining; }

        private byte soul;
        private byte lemureShroud;
        private byte voidShroud;
        private ushort enshroudedTimeRemaining;

        private const uint ReaperId = 39;

        public ReaperGaugeTracker(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/RPR";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != ReaperId)
                return;
            var gauge = Dalamud.JobGauges.Get<RPRGauge>();

            TestValue(gauge.Soul, ref soul);
            TestValue(gauge.LemureShroud, ref lemureShroud);
            TestValue(gauge.VoidShroud, ref voidShroud);
            TestCountDown(gauge.EnshroudedTimeRemaining, ref enshroudedTimeRemaining, 1000);

            PublishIfNeeded();
        }
    }
}
