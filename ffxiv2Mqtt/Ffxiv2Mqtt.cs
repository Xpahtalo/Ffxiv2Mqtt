using System;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics;

// Entry point for the plugin

namespace Ffxiv2Mqtt;

public class Ffxiv2Mqtt : IDalamudPlugin
{
    private const string        InternalName = "FFXIV2MQTT"; // Do not change this ever.
    private       Configuration Configuration { get; }
    private       PluginUI      PluginUi      { get; }

    private readonly MqttManager  mqttManager;
    private readonly TopicManager topicManager;
    private readonly PlayerEvents playerEvents;
    private readonly Ipc          ipc;
    public           string       Name => InternalName;

    private const string configCommandName = "/mqtt";
    private const string testCommandName   = "/mtest";
    private const string customCommandName = "/mqttcustom";
    
    private DalamudPluginInterface PluginInterface { get; init; }
    private CommandManager         CommandManager  { get; init; }

    public Ffxiv2Mqtt(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager)
    {
        PluginInterface = pluginInterface;
        CommandManager  = commandManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        mqttManager = new MqttManager(Configuration);
        if (Configuration.ConnectAtStartup)
            mqttManager.ConnectToBroker();
        
        CommandManager.AddHandler(configCommandName, new CommandInfo(OnCommand)
                                                                     {
                                                                         HelpMessage = "Display MQTT Client Info",
                                                                     });
        CommandManager.AddHandler(testCommandName, new CommandInfo(OnCommand)
                                                                   {
                                                                       HelpMessage = "Test",
                                                                       ShowInHelp  = false,
                                                                   });
        CommandManager.AddHandler(customCommandName, new CommandInfo(OnCommand)
                                                                     {
                                                                         HelpMessage =
                                                                             "Send a custom MQTT message with the given topic and payload.",
                                                                     });

        playerEvents = PluginInterface.Create<PlayerEvents>()!;
        topicManager = new TopicManager(mqttManager, Configuration);
        ipc          = PluginInterface.Create<Ipc>(mqttManager)!;

        foreach (var t in GetType().Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Topic))))
            try {
                PluginLog.Debug($"Adding {t.Name}");
                var topic = (Topic?)Activator.CreateInstance(t);
                if (topic is null) return;
                pluginInterface.Inject(topic, mqttManager, playerEvents, Configuration);
                topic.Initialize();
                topicManager.AddTopic(topic);
            } catch (Exception e) {
                PluginLog.Error($"Failed to create {t.Name}: {e}");
            }


        PluginUi = new PluginUI(Configuration, mqttManager, topicManager);
        
        PluginInterface.UiBuilder.Draw         += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    private void OnCommand(string command, string args)
    {
        PluginLog.Information($"Received command: {command}, with the args: {args}");
        switch (command) {
            case configCommandName:
                PluginUi.Visible = true;
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
        PluginUi.Draw();
    }

    private void DrawConfigUI()
    {
        PluginUi.SettingsVisible = true;
    }

    public void Dispose()
    {
        playerEvents.Dispose();

        topicManager.Clean();
        topicManager.Dispose();

        PluginUi.Dispose();
        CommandManager.RemoveHandler(configCommandName);
        CommandManager.RemoveHandler(testCommandName);
        CommandManager.RemoveHandler(customCommandName);
        mqttManager.Dispose();
    }
}
