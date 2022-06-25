using System.Threading;
using System.Threading.Tasks;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using Dalamud.Logging;
using System.IO;
using Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers;
using Ffxiv2Mqtt.TopicTracker;

using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "FFXIV2MQTT";

        private const string configCommandName = "/mqtt";
        private const string testCommandName = "/mtest";
        private const string customCommandName = "/mqttcustom";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }
        
        #region Plugin Services
        [PluginService]
        private Condition Condition { get; init; }
        [PluginService]
        private ClientState ClientState { get; init; }
        [PluginService]
        private DataManager DataManager { get; init; }
        [PluginService]
        private Framework Framework { get; init; }
        [PluginService]
        private JobGauges JobGauges { get; init; }
        #endregion

        private MqttManager mqttManager;
        
        private PlayerInfoTracker playerInfoTracker;
        private PlayerCombatStatsTracker playerCombatStatsTracker;
        private PlayerGathererStatsTracker playerGathererStatsTracker;
        private PlayerCrafterStatsTracker playerCrafterStatsTracker;
        private TerritoryTracker territoryTracker;

        // Tanks
        private PaladinGaugeTracker paladinGaugeTracker;
        private WarriorGaugeTracker warriorGaugeTracker;
        private DarkKnightGaugeTracker darkKnightGaugeTracker;
        private GunbreakerGaugeTracker gunbreakerGaugeTracker;
        // Healers
        private WhiteMageGaugeTracker whiteMageGaugeTracker;
        private AstrologianGaugeTracker astrologianGaugeTracker;
        private ScholarGaugeTracker scholarGaugeTracker;
        private SageGaugeTracker sageGaugeTracker;
        // Melee DPS
        private MonkGaugeTracker monkGaugeTracker;
        private DragoonGaugeTracker dragoonGaugeTracker;
        private NinjaGaugeTracker ninjaGaugeTracker;
        private SamuraiGaugeTracker samuraiGaugeTracker;
        private ReaperGaugeTracker reaperGaugeTracker;
        // Physical Ranged DPS
        private BardGaugeTracker bardGaugeTracker;
        private MachinistGaugeTracker machinistGaugeTracker;
        private DancerGaugeTracker dancerGaugeTracker;
        // Magical Ranged DPS
        private BlackMageGuageTracker blackMageGuageTracker;
        private SummonerGaugeTracker summonerGaugeTracker;
        private RedMageGaugeTracker redMageGaugeTracker;

        private const uint Thaumaturge = 7;
        private const uint Paladin = 19;
        private const uint Monk = 20;
        private const uint Warrior = 21;
        private const uint Dragoon = 22;
        private const uint Bard = 23;
        private const uint WhiteMage = 24;
        private const uint BlackMage = 25;
        private const uint Summoner = 27;
        private const uint Scholar = 28;
        private const uint Ninja = 30;
        private const uint Machinist = 31;
        private const uint DarkKnight = 32;
        private const uint Astrologian = 33;
        private const uint Samurai = 34;
        private const uint RedMage = 35;
        private const uint Gunbreaker = 37;
        private const uint Dancer = 38;
        private const uint Reaper = 39;
        private const uint Sage = 40;



        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] Condition condition,
            [RequiredVersion("1.0")] ClientState clientState,
            [RequiredVersion("1.0")] DataManager dataManager,
            [RequiredVersion("1.0")] Framework framework,
            [RequiredVersion("1.0")] JobGauges jobGauges)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.Condition = condition;
            this.ClientState = clientState;
            this.DataManager = dataManager;
            this.Framework = framework;
            this.JobGauges = jobGauges;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            mqttManager = new MqttManager(Configuration);
            if (Configuration.ConnectAtStartup)
                mqttManager.ConnectToBroker();

            this.Configuration.Initialize(this.PluginInterface);

            this.PluginUi = new PluginUI(this.Configuration, mqttManager);

            this.CommandManager.AddHandler(configCommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Display MQTT Client Info"
            });
            this.CommandManager.AddHandler(testCommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Test",
                ShowInHelp = false
            });
            this.CommandManager.AddHandler(customCommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Send a custom MQTT message with the given topic and payload."
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            Dalamud.Initialize(this.PluginInterface);
            playerInfoTracker = new PlayerInfoTracker(mqttManager);
            playerCombatStatsTracker = new PlayerCombatStatsTracker(mqttManager);
            playerGathererStatsTracker = new PlayerGathererStatsTracker(mqttManager);
            playerCrafterStatsTracker = new PlayerCrafterStatsTracker(mqttManager);
            territoryTracker = new TerritoryTracker(mqttManager);
            Dalamud.ClientState.TerritoryChanged += territoryTracker.TerritoryChanged;

            // Tanks
            paladinGaugeTracker = new PaladinGaugeTracker(this.mqttManager);
            warriorGaugeTracker = new WarriorGaugeTracker(this.mqttManager);
            darkKnightGaugeTracker = new DarkKnightGaugeTracker(this.mqttManager);
            gunbreakerGaugeTracker = new GunbreakerGaugeTracker(this.mqttManager);
            // Healer
            whiteMageGaugeTracker = new WhiteMageGaugeTracker(this.mqttManager);
            astrologianGaugeTracker = new AstrologianGaugeTracker(this.mqttManager);
            scholarGaugeTracker = new ScholarGaugeTracker(this.mqttManager);
            sageGaugeTracker = new SageGaugeTracker(this.mqttManager);
            // Melee DPS
            monkGaugeTracker = new MonkGaugeTracker(this.mqttManager);
            dragoonGaugeTracker = new DragoonGaugeTracker(this.mqttManager);
            ninjaGaugeTracker = new NinjaGaugeTracker(this.mqttManager);
            samuraiGaugeTracker = new SamuraiGaugeTracker(this.mqttManager);
            reaperGaugeTracker = new ReaperGaugeTracker(this.mqttManager);
            // Physical Ranged DPS
            bardGaugeTracker = new BardGaugeTracker(this.mqttManager);
            machinistGaugeTracker = new MachinistGaugeTracker(this.mqttManager);
            dancerGaugeTracker = new DancerGaugeTracker(this.mqttManager);
            // Magical Ranged DPS
            blackMageGuageTracker = new BlackMageGuageTracker(this.mqttManager);
            summonerGaugeTracker = new SummonerGaugeTracker(this.mqttManager);
            redMageGaugeTracker = new RedMageGaugeTracker(this.mqttManager);

            this.Condition.ConditionChange += ConditionChange;
            this.ClientState.CfPop += CfPop;
            this.ClientState.Login += Login;
            this.ClientState.Logout += Logout;
            this.Framework.Update += Update;
        }

        private void OnCommand(string command, string args)
        {
            if (command == configCommandName)
            {
                this.PluginUi.Visible = true;
            }
            else if (command == testCommandName)
            {
                mqttManager.PublishMessage("test", "success");
            }
            else if (command == customCommandName)
            {
                var argsList = args.Split(' ');

                if (argsList.Length < 2)
                {
                    PluginLog.LogError("Not enough arguments.");
                    return;
                }
                mqttManager.PublishMessage(argsList[0], argsList[1]);
            }
        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }

        public void Dispose()
        {
            mqttManager.PublishMessage("Player/Level", "", true);

            Condition.ConditionChange -= ConditionChange;
            ClientState.CfPop -= CfPop;
            ClientState.Login -= Login;
            ClientState.Logout -= Logout;
            Dalamud.ClientState.TerritoryChanged -= territoryTracker.TerritoryChanged;
            Framework.Update -= Update;

            PluginUi?.Dispose();
            CommandManager.RemoveHandler(configCommandName);
            CommandManager.RemoveHandler(testCommandName);
            CommandManager.RemoveHandler(customCommandName);
            mqttManager?.Dispose();
        }




        private void ConditionChange(ConditionFlag flag, bool value)
        {
            var topic = "Condition/" + flag.ToString();
            mqttManager.PublishMessage(topic, value.ToString());
        }

        private void CfPop(object? s, Lumina.Excel.GeneratedSheets.ContentFinderCondition e)
        {
            mqttManager.PublishMessage("ClientState/Queue", "Pop");
        }

        private void Login(object? s, System.EventArgs e)
        {
            mqttManager.PublishMessage("ClientState/Login", "LoggedIn", true);
            Task.Run(() =>
            {
                while (ClientState?.LocalPlayer?.Name == null)
                    Thread.Sleep(1000);
                mqttManager.PublishMessage("ClientState/LoggedInCharacter", ClientState.LocalPlayer.Name.ToString(), true);
            });
        }

        private void Logout(object? s, System.EventArgs e)
        {
            mqttManager.PublishMessage("ClientState/Login", "LoggedOut", true);
            mqttManager.PublishMessage("ClientState/LoggedInCharacter", string.Empty, true);
        }

        private void TerritoryChanged(object? s, ushort e)
        {
            var territoryName = DataManager?.GameData?.Excel?.GetSheet<Lumina.Excel.GeneratedSheets.TerritoryType>()?.GetRow(e)?.PlaceName?.Value?.Name;
            if (territoryName != null)
                mqttManager.PublishMessage("ClientState/TerritoryChanged", territoryName.ToString(), true);
        }

        private void Update(Framework framework)
        {
            var localPlayer = ClientState.LocalPlayer;
            if (localPlayer == null)
                return;

            playerInfoTracker.Update(localPlayer);
            playerCombatStatsTracker.Update(localPlayer);
            playerGathererStatsTracker.Update(localPlayer);
            playerCrafterStatsTracker.Update(localPlayer);

            if (!ClientState.IsPvP)
            {
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
                    case Gunbreaker:
                        gunbreakerGaugeTracker.Update(JobGauges.Get<GNBGauge>());
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
                    case Ninja:
                        ninjaGaugeTracker.Update(JobGauges.Get<NINGauge>());
                        break;
                    case Samurai:
                        samuraiGaugeTracker.Update(JobGauges.Get<SAMGauge>());
                        break;
                    case Reaper:
                        reaperGaugeTracker.Update(JobGauges.Get<RPRGauge>());
                        break;
                    // Physical Ranged DPS
                    case Bard:
                        bardGaugeTracker.Update(JobGauges.Get<BRDGauge>());
                        break;
                    case Machinist:
                        machinistGaugeTracker.Update(JobGauges.Get<MCHGauge>());
                        break;
                    case Dancer:
                        dancerGaugeTracker.Update(JobGauges.Get<DNCGauge>());
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
        }
    }
}