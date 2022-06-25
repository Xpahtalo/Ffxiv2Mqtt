using Dalamud.Logging;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class BardGaugeTracker : BaseGaugeTracker, IUpdatable
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

        private const uint BardId = 23;


        public BardGaugeTracker(MqttManager m) : base(m)
        {
            PluginLog.Verbose("Creating BardGaugeTracker");
            topic = "Player/JobGauge/BRD";
            coda = new Song[3];
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != BardId)
                return;
            var brdGauge = Dalamud.JobGauges.Get<BRDGauge>();

            for (var i = 0; i < coda.Length; i++)
            {
                TestValue(brdGauge.Coda[i], ref coda[i]);
            }
            TestValue(brdGauge.LastSong, ref lastSong);
            TestValue(brdGauge.Repertoire, ref repertoire);
            TestValue(brdGauge.Song, ref song);
            TestCountDown(brdGauge.SongTimer, ref songTimer, (ushort)synceTimer);
            TestValue(brdGauge.SoulVoice, ref soulVoice);

            PublishIfNeeded();
        }
    }
}
