using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class GunbreakerGaugeTracker : BaseGaugeTracker
    {
        private short ammo;
        private byte ammoComboStep;
        private short maxTimerDuration;

        public GunbreakerGaugeTracker(MqttManager m) : base(m)
        {
        }

        public void Update(GNBGauge gunbreakerGauge)
        {
            mqttManager.TestValue(gunbreakerGauge.Ammo, ref ammo, "JobGauge/GNB/Ammo");
            mqttManager.TestValue(gunbreakerGauge.AmmoComboStep, ref ammoComboStep, "JobGauge/GNB/AmmoComboStep");
            mqttManager.TestValue(gunbreakerGauge.MaxTimerDuration, ref maxTimerDuration, "JobGauge/GNB/MaxTimerDuration");
        }
    }
}
