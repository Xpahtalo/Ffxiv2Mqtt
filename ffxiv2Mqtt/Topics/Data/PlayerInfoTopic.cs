using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class PlayerInfoTopic : Topic, IUpdatable
    {
        public string Class { get => Dalamud.ClientState?.LocalPlayer?.ClassJob?.GameData?.Abbreviation ?? ""; }
        public uint ClassId { get => classJobId; }
        public byte Level { get => level; }
        public uint MaxHP { get => maxHP; }
        public uint MaxMP { get => maxMP; }
        public uint MaxCP { get => maxCP; }
        public uint MaxGP { get => maxGP; }

        uint classJobId;
        byte level;
        uint maxHP;
        uint maxMP;
        uint maxGP;
        uint maxCP;

        internal PlayerInfoTopic(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Info";
        }

        public void Update()
        {
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;

            TestValue(localPlayer.ClassJob.Id, ref classJobId);
            TestValue(localPlayer.Level, ref level);
            TestValue(localPlayer.MaxHp, ref maxHP);
            TestValue(localPlayer.MaxMp, ref maxMP);
            TestValue(localPlayer.MaxCp, ref maxCP);
            TestValue(localPlayer.MaxGp, ref maxGP);

            PublishIfNeeded();
        }
    }
}
