using Dalamud.Game.ClientState.Objects.SubKinds;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class PlayerCrafterStatsTracker : BaseTopicTracker
    {
        public uint CP { get => cp; }
        uint cp;

        internal PlayerCrafterStatsTracker(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Crafter/Current";
        }

        internal void Update(PlayerCharacter localPlayer)
        {
            TestValue(localPlayer.CurrentCp, ref cp);
            
            PublishIfNeeded();
        }
    }
}
