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

        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }


        private MqttManager mqttManager;
        private TopicManager trackerManager;


        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            Dalamud.Initialize(pluginInterface);
            this.Configuration = Dalamud.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            mqttManager = new MqttManager(Configuration);
            if (Configuration.ConnectAtStartup)
                mqttManager.ConnectToBroker();

            this.Configuration.Initialize(Dalamud.PluginInterface);


            Dalamud.CommandManager.AddHandler(configCommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Display MQTT Client Info"
            });
            Dalamud.CommandManager.AddHandler(testCommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Test",
                ShowInHelp = false
            });
            Dalamud.CommandManager.AddHandler(customCommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Send a custom MQTT message with the given topic and payload."
            });

            Dalamud.Initialize(Dalamud.PluginInterface);
            
            trackerManager = new TopicManager(mqttManager, Configuration);

            this.PluginUi = new PluginUI(this.Configuration, mqttManager, trackerManager);
            Dalamud.PluginInterface.UiBuilder.Draw += DrawUI;
            Dalamud.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            Dalamud.Framework.Update += Update;
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
                PluginLog.Information($"Publishing a custom message. topic: {argsList[0]} payload: {argsList[1]}");
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

            Dalamud.Framework.Update -= Update;

            PluginUi?.Dispose();
            Dalamud.CommandManager.RemoveHandler(configCommandName);
            Dalamud.CommandManager.RemoveHandler(testCommandName);
            Dalamud.CommandManager.RemoveHandler(customCommandName);
            mqttManager?.Dispose();
        }
    }
}