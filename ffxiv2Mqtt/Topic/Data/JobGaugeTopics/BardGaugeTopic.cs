using Ffxiv2Mqtt.Enums;
using Dalamud.Logging;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class BardGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public Song Song { get => song; }
        public ushort SongTimer { get => songTimer; }
        public byte SoulVoice { get => soulVoice; }
        public Song[] Coda { get => coda; }
        public byte Repertoire { get => repertoire; }
        public Song LastSong { get => lastSong; }

        private Song[] coda;
        private Song lastSong;
        private byte repertoire;
        private Song song;
        private ushort songTimer;
        private byte soulVoice;


        public BardGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/BRD";
            coda = new Song[3];
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.Bard)
                return;

            var brdGauge = DalamudServices.JobGauges.Get<BRDGauge>();
            for (var i = 0; i < coda.Length; i++)
            {
                TestValue(brdGauge.Coda[i], ref coda[i]);
            }
            TestValue(brdGauge.LastSong, ref lastSong);
            TestValue(brdGauge.Repertoire, ref repertoire);
            TestValue(brdGauge.Song, ref song);
            TestCountDown(brdGauge.SongTimer, ref songTimer, syncTimer);
            TestValue(brdGauge.SoulVoice, ref soulVoice);

            PublishIfNeeded();
        }
    }
}