using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class DragoonGaugeTracker : BaseGaugeTracker, IUpdatable
    {
        public byte EyeCount { get => eyeCount; }
        public byte FirstmindsFocusCount { get => firstmindsFocusCount; }
        public bool LotdActive { get => isLotdActive; }
        public short LotdTimer { get => lotdTimer; }

        private byte eyeCount;
        private byte firstmindsFocusCount;
        private bool isLotdActive;
        private short lotdTimer;

        private const uint DragoonId = 22;

        public DragoonGaugeTracker(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/DRG";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != DragoonId)
                return;
            var gauge = Dalamud.JobGauges.Get<DRGGauge>();

            TestValue(gauge.EyeCount, ref eyeCount);
            TestValue(gauge.FirstmindsFocusCount, ref firstmindsFocusCount);
            TestValue(gauge.IsLOTDActive, ref isLotdActive);
            TestCountDown(gauge.LOTDTimer, ref lotdTimer, (short)synceTimer);

            PublishIfNeeded();
        }
    }
}
