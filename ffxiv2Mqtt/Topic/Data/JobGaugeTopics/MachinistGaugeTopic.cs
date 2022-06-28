using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
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


        public MachinistGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/MCH";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.Machinist)
                return;
            
            var gauge = DalamudServices.JobGauges.Get<MCHGauge>();
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
