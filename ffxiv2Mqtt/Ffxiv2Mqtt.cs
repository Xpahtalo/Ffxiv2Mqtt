using System;
using System.Linq;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Logging;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics;

// Entry point for the plugin

namespace Ffxiv2Mqtt
{
    public class Ffxiv2Mqtt : IDalamudPlugin
    {
        public string Name => "FFXIV2MQTT";

        private const string configCommandName = "/mqtt";
        private const string testCommandName   = "/mtest";
        private const string customCommandName = "/mqttcustom";

        private Configuration Configuration { get; init; }
        private PluginUI      PluginUi      { get; init; }

        private readonly MqttManager  mqttManager;
        private readonly TopicManager topicManager;
        private readonly PlayerEvents playerEvents;


        public Ffxiv2Mqtt(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            DalamudServices.Initialize(pluginInterface);
            this.Configuration = DalamudServices.PluginInterface.GetPluginConfig() as Configuration ??
                                 new Configuration();
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
                                                                           ShowInHelp  = false
                                                                       });
            DalamudServices.CommandManager.AddHandler(customCommandName, new CommandInfo(OnCommand)
                                                                         {
                                                                             HelpMessage =
                                                                                 "Send a custom MQTT message with the given topic and payload."
                                                                         });

            playerEvents = pluginInterface.Create<PlayerEvents>()!;
            topicManager = new TopicManager(mqttManager, Configuration);

            foreach (var t in GetType().Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Topic)))) {
                try {
                    PluginLog.Debug($"Adding {t.Name}");
                    var topic = (Topic?)Activator.CreateInstance(t);
                    if (topic is null) return;
                    pluginInterface.Inject(topic, mqttManager, playerEvents, Configuration);
                    topic.Initialize();
                    topicManager.AddTopic(topic);
                } catch (Exception e) {
                    PluginLog.Error($"Failed to create {t.Name}: {e.ToString()}");
                }
            }


            this.PluginUi                                          =  new PluginUI(this.Configuration, mqttManager, topicManager);
            DalamudServices.PluginInterface.UiBuilder.Draw         += DrawUI;
            DalamudServices.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        private void OnCommand(string command, string args)
        {
            PluginLog.Information($"Received command: {command}, with the args: {args}");
            switch (command) {
                case configCommandName:
                    this.PluginUi.Visible = true;
                    break;
                case testCommandName:
                    mqttManager.PublishMessage("test", "success");
                    break;
                case customCommandName:
                {
                    var argsList = args.Split(' ');

                    if (argsList.Length < 2) {
                        PluginLog.LogError("Not enough arguments.");
                        return;
                    }

                    PluginLog.Information($"Publishing a custom message. topic: {argsList[0]} payload: {argsList[1]}");
                    mqttManager.PublishMessage(argsList[0], argsList[1]);
                    break;
                }
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
            playerEvents.Dispose();
            
            topicManager.Clean();
            topicManager.Dispose();

            PluginUi.Dispose();
            DalamudServices.CommandManager.RemoveHandler(configCommandName);
            DalamudServices.CommandManager.RemoveHandler(testCommandName);
            DalamudServices.CommandManager.RemoveHandler(customCommandName);
            mqttManager.Dispose();
        }
    }
}