using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Logging;
using Ffxiv2Mqtt.Topic;

// Entry point for the plugin

namespace Ffxiv2Mqtt
{
    public class Ffxiv2Mqtt : IDalamudPlugin
    {
        public string Name => "FFXIV2MQTT";

        private const string configCommandName = "/mqtt";
        private const string testCommandName = "/mtest";
        private const string customCommandName = "/mqttcustom";

        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        private readonly MqttManager mqttManager;
        private readonly TopicManager topicManager;


        public Ffxiv2Mqtt(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            DalamudServices.Initialize(pluginInterface);
            this.Configuration = DalamudServices.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(DalamudServices.PluginInterface);
            
            mqttManager = new MqttManager(Configuration);
            if (Configuration.ConnectAtStartup)
                mqttManager.ConnectToBroker();

            DalamudServices.CommandManager.AddHandler(configCommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Display MQTT Client Info"
            });
            DalamudServices.CommandManager.AddHandler(testCommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Test",
                ShowInHelp = false
            });
            DalamudServices.CommandManager.AddHandler(customCommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Send a custom MQTT message with the given topic and payload."
            });

            topicManager = new TopicManager(mqttManager, Configuration);

            this.PluginUi = new PluginUI(this.Configuration, mqttManager, topicManager);
            DalamudServices.PluginInterface.UiBuilder.Draw += DrawUI;
            DalamudServices.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            DalamudServices.Framework.Update += Update;
        }

        private void OnCommand(string command, string args)
        {
            PluginLog.Information($"Recieved command: {command}, with the args: {args}");
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
            topicManager.Update();
        }

        public void Dispose()
        {
            topicManager.Clean();

            DalamudServices.Framework.Update -= Update;

            PluginUi?.Dispose();
            DalamudServices.CommandManager.RemoveHandler(configCommandName);
            DalamudServices.CommandManager.RemoveHandler(testCommandName);
            DalamudServices.CommandManager.RemoveHandler(customCommandName);
            mqttManager?.Dispose();
        }
    }
}