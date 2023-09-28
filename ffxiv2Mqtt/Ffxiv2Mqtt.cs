using System;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Interface;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

// Entry point for the plugin

namespace Ffxiv2Mqtt;

public class Ffxiv2Mqtt : IDalamudPlugin
{
    private Configuration Configuration { get; }

    private readonly WindowSystem windowSystem;
    private readonly MainWindow   mainWindow;

    private readonly MqttManager  mqttManager;
    private readonly TopicManager topicManager;
    private readonly Ipc          ipc;

    private       DalamudPluginInterface PluginInterface { get; }
    private       ICommandManager        CommandManager  { get; }
    public        string                 Name            => InternalName;
    private const string                 InternalName = "FFXIV2MQTT"; // Do not change this ever.

    private const string ConfigCommandName = "/mqtt";
    private const string TestCommandName   = "/mtest";
    private const string CustomCommandName = "/mqttcustom";

    public Ffxiv2Mqtt(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager        commandManager)
    {
        PluginInterface = pluginInterface;
        CommandManager  = commandManager;

        PluginInterface.Create<Service>();
        Service.PlayerEvents = new PlayerEvents();

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        mqttManager = PluginInterface.Create<MqttManager>(Configuration)!;
        if (Configuration.ConnectAtStartup)
            mqttManager.ConnectToBroker();

        topicManager = new TopicManager(mqttManager, Configuration);
        ipc          = PluginInterface.Create<Ipc>(mqttManager)!;

        foreach (var t in GetType().Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Topic))))
            try {
                Service.Log.Debug($"Adding {t.Name}");
                var topic = (Topic?)Activator.CreateInstance(t);
                if (topic is null) return;
                pluginInterface.Inject(topic, mqttManager, Configuration);
                topicManager.AddTopic(topic);
            } catch (Exception e) {
                Service.Log.Error($"Failed to create {t.Name}: {e}");
            }

        windowSystem = new WindowSystem("Ffxiv2Mqtt");
        windowSystem.AddWindow(mainWindow = new MainWindow(Configuration, mqttManager, topicManager));

        PluginInterface.UiBuilder.Draw         += DrawMainWindow;
        PluginInterface.UiBuilder.OpenConfigUi += OpenMainWindow;

        CommandManager.AddHandler(ConfigCommandName, new CommandInfo(OnCommand)
                                                     {
                                                         HelpMessage = "Display MQTT Client Info",
                                                     });
        CommandManager.AddHandler(TestCommandName, new CommandInfo(OnCommand)
                                                   {
                                                       HelpMessage = "Test",
                                                       ShowInHelp  = false,
                                                   });
        CommandManager.AddHandler(CustomCommandName, new CommandInfo(OnCommand)
                                                     {
                                                         HelpMessage =
                                                             "Send a custom MQTT message with the given topic and payload.",
                                                     });
    }

    private void OnCommand(string command, string args)
    {
        Service.Log.Information($"Received command: {command}, with the args: {args}");
        switch (command) {
            case ConfigCommandName:
                mainWindow.IsOpen = true;
                break;
            case TestCommandName:
                mqttManager.PublishMessage("test", "success");
                break;
            case CustomCommandName:
            {
                var argsList = args.Split(' ');

                if (argsList.Length < 2) {
                    Service.Log.Error("Not enough arguments.");
                    return;
                }

                Service.Log.Information($"Publishing a custom message. topic: {argsList[0]} payload: {argsList[1]}");
                mqttManager.PublishMessage(argsList[0], argsList[1]);
                break;
            }
        }
    }

    private void DrawMainWindow() { windowSystem.Draw(); }

    private void OpenMainWindow() { mainWindow.IsOpen = true; }

    public void Dispose()
    {
        ipc.Dispose();

        Service.PlayerEvents.Dispose();

        topicManager.Clean();
        topicManager.Dispose();

        CommandManager.RemoveHandler(ConfigCommandName);
        CommandManager.RemoveHandler(TestCommandName);
        CommandManager.RemoveHandler(CustomCommandName);
        mqttManager.Dispose();
    }
}
