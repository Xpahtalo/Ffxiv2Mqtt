using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class ScholarGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte Aetherflow { get => aetherflow; }
        public DismissedFairy DismissedFairy { get => dismissedFairy; }
        public byte FairyGauge { get => fairyGauge; }
        public short SeraphTimer { get => seraphTimer; }

        private byte aetherflow;
        private DismissedFairy dismissedFairy;
        private byte fairyGauge;
        private short seraphTimer;


        public ScholarGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/SCH";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.Scholar)
                return;
            
            var gauge = DalamudServices.JobGauges.Get<SCHGauge>();
            TestValue(gauge.Aetherflow, ref aetherflow);
            TestValue(gauge.DismissedFairy, ref dismissedFairy);
            TestValue(gauge.FairyGauge, ref fairyGauge);
            TestCountDown(gauge.SeraphTimer, ref seraphTimer, syncTimer);

            PublishIfNeeded();
        }
    }
}
