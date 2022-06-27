using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class PlayerGathererStatsTopic : Topic, IUpdatable
    {
        public uint GP { get => gp; }
        uint gp;

        internal PlayerGathererStatsTopic(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Gatherer/CurrentStats";
        }

        public void Update()
        {
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;

            if (localPlayer.MaxGp != 0)
                TestValue(localPlayer.CurrentGp, ref gp);

            PublishIfNeeded();
        }
    }
}
