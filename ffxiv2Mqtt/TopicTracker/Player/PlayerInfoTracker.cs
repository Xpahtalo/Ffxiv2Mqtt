using Dalamud.Game.ClientState.Objects.SubKinds;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.ClientState.Resolvers;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class PlayerInfoTracker : BaseTopicTracker
    {
        public byte Level { get => level; }
        byte level;
        public uint MaxHP { get => maxHP; }
        uint maxHP;
        public uint MaxMP { get => maxMP; }
        uint maxMP;
        public uint MaxCP { get => maxCP; }
        uint maxCP;
        public uint MaxGP { get => maxGP; }
        uint maxGP;
        public string Class { get => classJob.Abbreviation; }
        ClassJob classJob;

        internal PlayerInfoTracker(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Info";
        }

        internal void Update(PlayerCharacter localPlayer)
        {
            var currentClassJob = localPlayer.ClassJob.GameData;
            if (currentClassJob is not null)
                TestValue<ClassJob>(currentClassJob, ref classJob);
            TestValue(localPlayer.Level, ref level);
            TestValue(localPlayer.MaxHp, ref maxHP);
            TestValue(localPlayer.MaxMp, ref maxMP);
            TestValue(localPlayer.MaxCp, ref maxCP);
            TestValue(localPlayer.MaxGp, ref maxGP);

            PublishIfNeeded();
        }
    }
}
