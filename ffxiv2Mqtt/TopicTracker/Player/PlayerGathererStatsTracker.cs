using Newtonsoft.Json;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class PlayerGathererStatsTracker : BaseTopicTracker
    {
        public uint GP { get => gp; }
        uint gp;
        
        internal PlayerGathererStatsTracker(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Gatherer/Current";
        }

        internal void Update(PlayerCharacter localPlayer)
        {
            if (localPlayer.MaxGp != 0)
                TestValue(localPlayer.CurrentGp, ref gp);

            PublishIfNeeded();
        }
    }
}
