using System;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
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

    private readonly TopicManager topicManager;
    private readonly Ipc          ipc;

    private       IDalamudPluginInterface PluginInterface { get; }
    private       ICommandManager        CommandManager  { get; }
    public        string                 Name            => InternalName;
    private const string                 InternalName = "FFXIV2MQTT"; // Do not change this ever.

    private const string ConfigCommandName = "/mqtt";
    private const string TestCommandName   = "/mtest";
    private const string CustomCommandName = "/mqttcustom";

    public Ffxiv2Mqtt(
        IDalamudPluginInterface pluginInterface,
        ICommandManager        commandManager)
    {
        PluginInterface = pluginInterface;
        CommandManager  = commandManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        
        PluginInterface.Create<Service>();
        Service.PlayerEvents = new PlayerEvents();
        Service.MqttManager  = new MqttManager(Configuration);

        if (Configuration.ConnectAtStartup)
            Service.MqttManager.ConnectToBroker();

        topicManager = new TopicManager(Configuration);
        ipc          = new Ipc();

        foreach (var t in GetType().Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Topic))))
            try {
                Service.Log.Debug($"Adding {t.Name}");
                var topic = (Topic?)Activator.CreateInstance(t);
                if (topic is null)
                    return;
                pluginInterface.Inject(topic, Configuration);
                topicManager.AddTopic(topic);
            } catch (Exception e) {
                Service.Log.Error($"Failed to create {t.Name}: {e}");
            }
        topicManager.Configure(Configuration);
        
        windowSystem = new WindowSystem("Ffxiv2Mqtt");
        windowSystem.AddWindow(mainWindow = new MainWindow(Configuration, topicManager));

        PluginInterface.UiBuilder.Draw         += DrawMainWindow;
        PluginInterface.UiBuilder.OpenConfigUi += OpenMainWindow;

        CommandManager.AddHandler(ConfigCommandName, new CommandInfo(OnCommand)
                                                     {
                                                         HelpMessage = "Open plugin settings.",
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
                Service.MqttManager.PublishMessage("test", "success");
                break;
            case CustomCommandName:
            {
                var argsList = args.Split(' ');

                if (argsList.Length < 2) {
                    Service.Log.Error("Not enough arguments.");
                    return;
                }

                Service.Log.Information($"Publishing a custom message. topic: {argsList[0]} payload: {argsList[1]}");
                Service.MqttManager.PublishMessage(argsList[0], argsList[1]);
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
        Service.MqttManager.Dispose();
    }
}
