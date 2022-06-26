using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Logging;
using Ffxiv2Mqtt.TopicTracker;


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

        [PluginService]
        private Framework Framework { get; init; }

        private MqttManager mqttManager;
        private TrackerManager trackerManager;


        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] Framework framework)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.Framework = framework;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            mqttManager = new MqttManager(Configuration);
            if (Configuration.ConnectAtStartup)
                mqttManager.ConnectToBroker();

            this.Configuration.Initialize(this.PluginInterface);


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

            Dalamud.Initialize(this.PluginInterface);
            
            trackerManager = new TrackerManager(mqttManager, Configuration);

            this.PluginUi = new PluginUI(this.Configuration, mqttManager, trackerManager);
            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
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

        private void Update(Framework framework)
        {
            trackerManager.Update();
        }

        public void Dispose()
        {
            trackerManager.Clean();

            Framework.Update -= Update;

            PluginUi?.Dispose();
            CommandManager.RemoveHandler(configCommandName);
            CommandManager.RemoveHandler(testCommandName);
            CommandManager.RemoveHandler(customCommandName);
            mqttManager?.Dispose();
        }
    }
}