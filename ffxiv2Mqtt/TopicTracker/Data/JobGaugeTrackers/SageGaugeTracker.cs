using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class SageGaugeTracker : BaseGaugeTracker, IUpdatable
    {
        public byte Addersgall { get => addersgall; }
        public short AddersgallTimer { get => addersgallTimer; }
        public byte Addersting { get => addersting; }
        public bool Eukrasia { get => eukrasia; }

        private byte addersgall;
        private short addersgallTimer;
        private byte addersting;
        private bool eukrasia;

        private const uint SageId = 40;

        public SageGaugeTracker(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/SGE";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != SageId)
                return;
            var gauge = Dalamud.JobGauges.Get<SGEGauge>();

            TestValue(gauge.Addersgall, ref addersgall);
            TestCountUp(gauge.AddersgallTimer, ref addersgallTimer, (short)synceTimer);
            TestValue(gauge.Addersting, ref addersting);
            TestValue(gauge.Eukrasia, ref eukrasia);

            PublishIfNeeded();
        }
    }
}
