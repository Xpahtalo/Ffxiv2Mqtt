using System;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace Ffxiv2Mqtt.Services;

public sealed class Ipc : IDisposable
{
    public const string LabelProviderReady          = "Ffxiv2Mqtt.Ready";
    public const string LabelProviderPublishMessage = "Ffxiv2Mqtt.Publish";

    // Always returns ready so that other plugins can check without having to do odd Dalamud checks
    public ICallGateProvider<bool>? ProviderReady;

    // Publish a message.
    // First string is the topic path.
    // Second string in the payload.
    // Return is if it was successfully added to the queue, NOT if it was successfully published.
    public ICallGateProvider<string, string, bool>? ProviderPublishMessage;

    [PluginService] public MqttManager?            MqttManager     { get; set; }
    [PluginService] public DalamudPluginInterface? PluginInterface { get; set; }


    public Ipc()
    {
        PluginLog.Verbose("Registering IPC Providers.");

        try {
            ProviderReady = PluginInterface!.GetIpcProvider<bool>(LabelProviderReady);
            ProviderReady.RegisterFunc(Ready);
        } catch (Exception ex) {
            PluginLog.Error($"(Failed to register IPC provider for {LabelProviderReady}: {ex}");
        }

        try {
            ProviderPublishMessage = PluginInterface!.GetIpcProvider<string, string, bool>(LabelProviderPublishMessage);
            ProviderPublishMessage.RegisterFunc(Publish);
        } catch (Exception ex) {
            PluginLog.Error($"Failed to register IPC provider for {LabelProviderPublishMessage}: {ex}");
        }
    }

    public void Dispose()
    {
        ProviderReady?.UnregisterFunc();
        ProviderPublishMessage?.UnregisterFunc();
    }

    private bool Ready()
    {
        PluginLog.Information("Received IPC ready request.");
        return true;
    }

    private bool Publish(string topic, string payload)
    {
        PluginLog.Verbose($"{LabelProviderPublishMessage} received message. Publishing {payload} to {topic}");
        MqttManager!.PublishMessage(topic, payload);
        return true;
    }
}
