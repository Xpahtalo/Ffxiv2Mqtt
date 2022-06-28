using Dalamud.Logging;
using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class GunbreakerGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public short Ammo { get => ammo; }
        public byte AmmoComboStep { get => ammoComboStep; }
        public short MaxTimerDuration { get => maxTimerDuration; }

        private short ammo;
        private byte ammoComboStep;
        private short maxTimerDuration;

        private const uint GunbreakerId = 37;

        public GunbreakerGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/GNB";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != GunbreakerId)
                return;
            var gauge = DalamudServices.JobGauges.Get<GNBGauge>();

            TestValue(gauge.Ammo, ref ammo);
            TestValue(gauge.AmmoComboStep, ref ammoComboStep);
            TestValue(gauge.MaxTimerDuration, ref maxTimerDuration);

            PublishIfNeeded();
        }
    }
}
