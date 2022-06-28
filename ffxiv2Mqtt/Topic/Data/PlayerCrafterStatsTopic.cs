using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class PlayerCrafterStatsTopic : Topic, IUpdatable
    {
        public uint CP { get => cp; }
        uint cp;

        internal PlayerCrafterStatsTopic(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Crafter/CurrentStats";
        }

        public void Update()
        {
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;

            TestValue(localPlayer.CurrentCp, ref cp);

            PublishIfNeeded();
        }
    }
}
