namespace Ffxiv2Mqtt.TopicTracker
{
    internal class PlayerInfoTracker : BaseTopicTracker, IUpdatable
    {
        public string Class { get => Dalamud.ClientState?.LocalPlayer?.ClassJob?.GameData?.Abbreviation ?? ""; }
        uint classJobId;
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


        internal PlayerInfoTracker(MqttManager mqttManager) : base(mqttManager)
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
