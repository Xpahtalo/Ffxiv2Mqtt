using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
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
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;

            TestValue(localPlayer.CurrentCp, ref cp);

            PublishIfNeeded();
        }
    }
}
