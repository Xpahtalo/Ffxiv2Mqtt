using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers;

namespace Ffxiv2Mqtt.EventHandlers
{
    internal class JobGaugeHandler
    {
        public static void Initialize(DalamudPluginInterface pluginInterface) =>
            pluginInterface.Create<JobGaugeHandler>();

        #region Plugin Services
        [PluginService]
        [RequiredVersion("1.0")]
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ClientState ClientState { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static JobGauges JobGauges { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static Framework Framework { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static DataManager DataManager { get; private set; } = null!;
        #endregion

        private MqttManager mqttManager;

        // Tanks
        private PaladinGaugeTracker     paladinGaugeTracker;
        private WarriorGaugeTracker     warriorGaugeTracker;
        private DarkKnightGaugeTracker  darkKnightGaugeTracker;
        // Healers
        private WhiteMageGaugeTracker   whiteMageGaugeTracker;
        private AstrologianGaugeTracker astrologianGaugeTracker;
        private ScholarGaugeTracker     scholarGaugeTracker;
        private SageGaugeTracker        sageGaugeTracker;
        // Melee DPS
        private MonkGaugeTracker        monkGaugeTracker;
        private DragoonGaugeTracker     dragoonGaugeTracker;
        private SamuraiGaugeTracker     samuraiGaugeTracker;
        private ReaperGaugeTracker      reaperGaugeTracker;
        // Physical Ranged DPS
        private MachinistGaugeTracker   machinistGaugeTracker;
        // Magical Ranged DPS
        private BlackMageGuageTracker   blackMageGuageTracker;
        private SummonerGaugeTracker    summonerGaugeTracker;
        private RedMageGaugeTracker     redMageGaugeTracker;

        private const uint Thaumaturge  = 7;
        private const uint Paladin      = 19;
        private const uint Monk         = 20;
        private const uint Warrior      = 21;
        private const uint Dragoon      = 22;
        private const uint Bard         = 23;
        private const uint WhiteMage    = 24;
        private const uint BlackMage    = 25;
        private const uint Summoner     = 27;
        private const uint Scholar      = 28;
        private const uint Ninja        = 30;
        private const uint Machinist    = 31;
        private const uint DarkKnight   = 32;
        private const uint Astrologian  = 33;
        private const uint Samurai      = 34;
        private const uint RedMage      = 35;
        private const uint Gunbreaker   = 37;
        private const uint Dancer       = 38;
        private const uint Reaper       = 39;
        private const uint Sage         = 40;


        public JobGaugeHandler(DalamudPluginInterface pluginInterface, MqttManager mqttManager)
        {
            JobGaugeHandler.Initialize(pluginInterface);
            this.mqttManager = mqttManager;

            // Tanks
            paladinGaugeTracker     = new PaladinGaugeTracker(this.mqttManager);
            warriorGaugeTracker     = new WarriorGaugeTracker(this.mqttManager);
            darkKnightGaugeTracker  = new DarkKnightGaugeTracker(this.mqttManager);
            // Healer
            whiteMageGaugeTracker   = new WhiteMageGaugeTracker(this.mqttManager);
            astrologianGaugeTracker = new AstrologianGaugeTracker(this.mqttManager);
            scholarGaugeTracker     = new ScholarGaugeTracker(this.mqttManager);
            sageGaugeTracker        = new SageGaugeTracker(this.mqttManager);
            // Melee DPS
            monkGaugeTracker        = new MonkGaugeTracker(this.mqttManager);
            dragoonGaugeTracker     = new DragoonGaugeTracker(this.mqttManager);
            samuraiGaugeTracker     = new SamuraiGaugeTracker(this.mqttManager);
            reaperGaugeTracker      = new ReaperGaugeTracker(this.mqttManager);
            // Physical Ranged DPS
            machinistGaugeTracker   = new MachinistGaugeTracker(this.mqttManager);
            // Magical Ranged DPS
            blackMageGuageTracker   = new BlackMageGuageTracker(this.mqttManager);
            summonerGaugeTracker    = new SummonerGaugeTracker(this.mqttManager);
            redMageGaugeTracker     = new RedMageGaugeTracker(this.mqttManager);

            Framework.Update += Update;
        }

        private void Update(Framework framework)
        {
            if (ClientState.IsPvP) return;

            var localPlayer = ClientState.LocalPlayer;
            if (localPlayer == null) return;

            switch (localPlayer.ClassJob.Id)
            {
                // Tanks
                case Paladin:
                    paladinGaugeTracker.Update(JobGauges.Get<PLDGauge>());
                    break;
                case Warrior:
                    warriorGaugeTracker.Update(JobGauges.Get<WARGauge>());
                    break;
                case DarkKnight:
                    darkKnightGaugeTracker.Update(JobGauges.Get<DRKGauge>());
                    break;
                // Healers
                case WhiteMage:
                    whiteMageGaugeTracker.Update(JobGauges.Get<WHMGauge>());
                    break;
                case Astrologian:
                    astrologianGaugeTracker.Update(JobGauges.Get<ASTGauge>());
                    break;
                case Scholar:
                    scholarGaugeTracker.Update(JobGauges.Get<SCHGauge>());
                    break;
                case Sage:
                    sageGaugeTracker.Update(JobGauges.Get<SGEGauge>());
                    break;
                // Melee DPS
                case Monk:
                    monkGaugeTracker.Update(JobGauges.Get<MNKGauge>());
                    break;
                case Dragoon:
                    dragoonGaugeTracker.Update(JobGauges.Get<DRGGauge>());
                    break;
                case Samurai:
                    samuraiGaugeTracker.Update(JobGauges.Get<SAMGauge>());
                    break;
                case Reaper:
                    reaperGaugeTracker.Update(JobGauges.Get<RPRGauge>());
                    break;
                // Physical Ranged DPS
                case Machinist:
                    machinistGaugeTracker.Update(JobGauges.Get<MCHGauge>());
                    break;
                // Magical Ranged DPS
                case Thaumaturge: // Thaumaturge also uses the Black Mage Job Gauge
                    goto case BlackMage;
                case BlackMage:
                    blackMageGuageTracker.Update(JobGauges.Get<BLMGauge>());
                    break;
                case Summoner:
                    summonerGaugeTracker.Update(JobGauges.Get<SMNGauge>());
                    break;
                case RedMage:
                    redMageGaugeTracker.Update(JobGauges.Get<RDMGauge>());
                    break;
                default:
                    return;
            }
        }

        public void Dispose()
        {
            Framework.Update -= Update;
        }
    }
}
