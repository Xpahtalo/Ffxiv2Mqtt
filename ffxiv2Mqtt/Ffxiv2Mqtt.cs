using System;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Ffxiv2Mqtt.Interface;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics;

// Entry point for the plugin

namespace Ffxiv2Mqtt;

public class Ffxiv2Mqtt : IDalamudPlugin
{
    private Configuration Configuration { get; }

    private readonly WindowSystem windowSystem;
    private readonly MainWindow   mainWindow;

    private readonly MqttManager  mqttManager;
    private readonly TopicManager topicManager;
    private readonly PlayerEvents playerEvents;
    private readonly Ipc          ipc;

    private       DalamudPluginInterface PluginInterface { get; }
    private       CommandManager         CommandManager  { get; }
    public        string                 Name            => InternalName;
    private const string                 InternalName = "FFXIV2MQTT"; // Do not change this ever.

    private const string configCommandName = "/mqtt";
    private const string testCommandName   = "/mtest";
    private const string customCommandName = "/mqttcustom";

    public Ffxiv2Mqtt(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager         commandManager)
    {
        PluginInterface = pluginInterface;
        CommandManager  = commandManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        mqttManager = PluginInterface.Create<MqttManager>(Configuration)!;
        if (Configuration.ConnectAtStartup)
            mqttManager.ConnectToBroker();

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

        windowSystem = new WindowSystem("Ffxiv2Mqtt");
        windowSystem.AddWindow(mainWindow = new MainWindow(Configuration, mqttManager, topicManager));

        PluginInterface.UiBuilder.Draw         += DrawMainWindow;
        PluginInterface.UiBuilder.OpenConfigUi += OpenMainWindow;

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
    }

    private void OnCommand(string command, string args)
    {
        PluginLog.Information($"Received command: {command}, with the args: {args}");
        switch (command) {
            case configCommandName:
                mainWindow.IsOpen = true;
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

    private void DrawMainWindow()
    {
        windowSystem.Draw();
    }

    private void OpenMainWindow()
    {
        mainWindow.IsOpen = true;
    }

    public void Dispose()
    {
        ipc.Dispose();

        playerEvents.Dispose();

        topicManager.Clean();
        topicManager.Dispose();

        CommandManager.RemoveHandler(configCommandName);
        CommandManager.RemoveHandler(testCommandName);
        CommandManager.RemoveHandler(customCommandName);
        mqttManager.Dispose();
    }
}
