using Dalamud.Game.ClientState.Objects.SubKinds;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class PlayerCombatStatsTracker : BaseTopicTracker
    {
        public uint HP { get => hp; }
        public uint MP { get => mp; }
        uint hp;
        uint mp;

        internal PlayerCombatStatsTracker(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Combat/Current";
        }

        internal void Update(PlayerCharacter localPlayer)
        {
            TestValue(localPlayer.CurrentHp, ref hp);
            TestValue(localPlayer.CurrentMp, ref mp);

            PublishIfNeeded();
        }
    }
}
