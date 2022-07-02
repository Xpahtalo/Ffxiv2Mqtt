using Ffxiv2Mqtt.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class DragoonGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte EyeCount { get => eyeCount; }
        public byte FirstmindsFocusCount { get => firstmindsFocusCount; }
        public bool LotdActive { get => isLotdActive; }
        public short LotdTimer { get => lotdTimer; }

        private byte eyeCount;
        private byte firstmindsFocusCount;
        private bool isLotdActive;
        private short lotdTimer;

        
        public DragoonGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/DRG";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.Dragoon)
                return;
            
            var gauge = DalamudServices.JobGauges.Get<DRGGauge>();
            TestValue(gauge.EyeCount, ref eyeCount);
            TestValue(gauge.FirstmindsFocusCount, ref firstmindsFocusCount);
            TestValue(gauge.IsLOTDActive, ref isLotdActive);
            TestCountDown(gauge.LOTDTimer, ref lotdTimer, syncTimer);

            PublishIfNeeded();
        }
    }
}
