using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class BardGaugeTracker : BaseGaugeTracker
    {
        private Song[] coda;
        private Song lastSong;
        private byte repertoire;
        private Song song;
        private ushort songTimer;
        private byte soulVoice;

        public BardGaugeTracker(MqttManager m) : base(m)
        {
            coda = new Song[3];
        }

        public void Update(BRDGauge bardGauge)
        {
            for (var i = 0; i < coda.Length; i++)
            {
                mqttManager.TestValue(bardGauge.Coda[i], ref coda[i], string.Format("JobGauge/BRD/Coda{0}", i + 1));
            }
            mqttManager.TestValue(bardGauge.LastSong, ref lastSong, "JobGauge/BRD/LastSong");
            mqttManager.TestValue(bardGauge.Repertoire, ref repertoire, "JobGauge/BRD/Repertoire");
            mqttManager.TestValue(bardGauge.Song, ref song, "JobGauge/BRD/Song");
            mqttManager.TestCountDown(bardGauge.SongTimer, ref songTimer, 1000, "JobGauge/BRD/SongTimer");
            mqttManager.TestValue(bardGauge.SoulVoice, ref soulVoice, "JobGauge/BRD/SoulVoice");
        }
    }
}
