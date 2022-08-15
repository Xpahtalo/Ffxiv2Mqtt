using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace Ffxiv2Mqtt.Services;

public sealed class Ipc : IDisposable
{
    // Returns if the MQTT service is started.
    private readonly ICallGateProvider<bool>? providerReady;

    // Publish a message.
    // First string is the topic path.
    // Second string is the payload.
    // Return is if it was successfully added to the queue, NOT if it was successfully published.
    private readonly ICallGateProvider<string, string, bool>? providerPublishMessage;

    // This exposes some extra MQTT message options for publishing messages. It's a bit hacky right now, but I will 
    // clean it up and make it nicer to use if anyone reaches out to me.
    // First argument is the topic path.
    // Second argument is the payload.
    // Third argument is if it just have the retained flag.
    // Fourth argument is the QoL level.
    // - 0 is At Most Once
    // - 1 is At Least Once
    // - 2 is Exactly Once
    // Return is if it was successfully added to the queue, NOT if it was successfully published.
    private readonly ICallGateProvider<string, string, bool, int, bool>? providerPublishMessageWithOptions;

    // Requests a subscription.
    // The first string is the IPC label of your ICallGateProvider that FFXIV2MQTT will send it's messages to.
    // The second string is the topic you would like to subscribe to. Currently, these are automatically adjusted to the
    // client's topic path. For example, if you request "tippy/tips" you will actually be subscribed to "ffxiv/tippy/tips".
    // Return is if it was successfully subscribed.
    private readonly ICallGateProvider<string, string, bool>? providerRequestSubscription;

    private Dictionary<string, ICallGateSubscriber<string, bool>> callGateSubscribers;

    [PluginService] public MqttManager?            MqttManager     { get; set; }
    [PluginService] public DalamudPluginInterface? PluginInterface { get; set; }
    
    private const string LabelProviderReady                 = "Ffxiv2Mqtt.Ready";
    private const string LabelProviderPublishMessage        = "Ffxiv2Mqtt.Publish";
    private const string LabelProviderPublishMessageOptions = "Ffxiv2Mqtt.PublishWithOptions";
    private const string LabelProviderRequestSubscription   = "Ffxiv2Mqtt.RequestSubscription";


    public Ipc()
    {
        PluginLog.Verbose("Registering IPC Providers.");

        try {
            providerReady = PluginInterface!.GetIpcProvider<bool>(LabelProviderReady);
            providerReady.RegisterFunc(Ready);
        } catch (Exception ex) {
            PluginLog.Error($"(Failed to register IPC provider for {LabelProviderReady}: {ex}");
        }

        try {
            providerPublishMessage = PluginInterface!.GetIpcProvider<string, string, bool>(LabelProviderPublishMessage);
            providerPublishMessage.RegisterFunc(Publish);
        } catch (Exception ex) {
            PluginLog.Error($"Failed to register IPC provider for {LabelProviderPublishMessage}: {ex}");
        }

        try {
            providerPublishMessageWithOptions = PluginInterface!.GetIpcProvider<string, string, bool, int, bool>(LabelProviderPublishMessageOptions);
            providerPublishMessageWithOptions.RegisterFunc(PublishWithOptions);
        } catch (Exception ex) {
            PluginLog.Error($"Failed to register IPC provider for {LabelProviderPublishMessageOptions}: {ex}");
        }

        try {
            providerRequestSubscription = PluginInterface!.GetIpcProvider<string, string, bool>(LabelProviderRequestSubscription);
            providerRequestSubscription.RegisterFunc(RequestSubscription);
        } catch (Exception ex) {
            PluginLog.Error($"Failed to register IPC provider for {LabelProviderRequestSubscription}: {ex}");
        }

        callGateSubscribers = new Dictionary<string, ICallGateSubscriber<string, bool>>();
        
        MqttManager!.AddMessageReceivedHandler(IpcMessageReceivedHandler);
    }

    public void Dispose()
    {
        providerReady?.UnregisterFunc();
        providerPublishMessage?.UnregisterFunc();
    }

    private bool Ready()
    {
        PluginLog.Information("Received IPC ready request.");
        return MqttManager?.IsStarted ?? false;
    }

    private bool Publish(string topic, string payload)
    {
        PluginLog.Verbose($"{LabelProviderPublishMessage} received message. Publishing {payload} to {topic}");
        MqttManager!.PublishMessage(topic, payload);
        return true;
    }

    private bool PublishWithOptions(string topic, string payload, bool retained, int qos)
    {
        PluginLog.Verbose($"{LabelProviderPublishMessageOptions} received message. Publishing {payload} to {topic}");

        MqttQualityOfServiceLevel qosLevel;
        
        try {
            qosLevel = ToQolLevel(qos);
        } catch (ArgumentOutOfRangeException ex) {
            PluginLog.Error($"Invalid Arguments. Will not publish message./n{ex.Message}");
            return false;
        }

        MqttManager!.PublishMessage(topic, payload, retained, qosLevel);
        return true;
    }

    private MqttQualityOfServiceLevel ToQolLevel(int qos) =>
        qos switch
        {
            0 => MqttQualityOfServiceLevel.AtMostOnce,
            1 => MqttQualityOfServiceLevel.AtLeastOnce,
            2 => MqttQualityOfServiceLevel.ExactlyOnce,
            _ => throw new ArgumentOutOfRangeException($"{qos} out of range of MqttQualityOfServiceLevel"),
        };
    private bool RequestSubscription(string label, string topic)
    {
        try {
            var callGateSubscriber = PluginInterface!.GetIpcSubscriber<string, bool>(label);
            callGateSubscribers.Add(topic, callGateSubscriber);
        } catch (Exception ex) {
            PluginLog.Error($"Failed to add a new subscription from IPC:/n{ex}");
            return false;
        }

        return true;
    }

    private Task IpcMessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic   = e.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload, 0, e.ApplicationMessage.Payload.Length);

        callGateSubscribers[topic].InvokeFunc(payload);

        return Task.CompletedTask;
    }
}
