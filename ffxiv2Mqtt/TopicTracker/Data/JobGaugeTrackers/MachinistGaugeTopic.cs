using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class MachinistGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte Battery { get => battery; }
        public byte Heat { get => heat; }
        public bool Overheated { get => isOverheated; }
        public short OverheatTimeRemaining { get => overheatTimeRemaining; }
        public bool RobotActive { get => isRobotActive; }
        public short SummonTimeRemaining { get => summonTimeRemaining; }
        public byte LastSummonBatteryPower { get => lastSummonBatteryPower; }

        private byte battery;
        private byte heat;
        private bool isOverheated;
        private short overheatTimeRemaining;
        private bool isRobotActive;
        private short summonTimeRemaining;
        private byte lastSummonBatteryPower;

        private const uint MachinistId = 31;

        public MachinistGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/MCH";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != MachinistId)
                return;
            var gauge = Dalamud.JobGauges.Get<MCHGauge>();

            TestValue(gauge.Battery, ref battery);
            TestValue(gauge.Heat, ref heat);
            TestValue(gauge.IsOverheated, ref isOverheated);
            TestValue(gauge.IsRobotActive, ref isRobotActive);
            TestValue(gauge.LastSummonBatteryPower, ref lastSummonBatteryPower);
            TestCountDown(gauge.OverheatTimeRemaining, ref overheatTimeRemaining, syncTimer);
            TestCountDown(gauge.SummonTimeRemaining, ref summonTimeRemaining, syncTimer);

            PublishIfNeeded();
        }
    }
}
