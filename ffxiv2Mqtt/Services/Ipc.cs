using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Plugin.Ipc;
using MQTTnet;
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

    // Publish a message. This is the preferred methode of publishing messages. The previous may be removed at some point.
    // First argument is a message object.
    // Return is if it was successfully added to the queue, NOT if it was successfully published.
    private readonly ICallGateProvider<Message, bool>? providerPublishMessageV2;

    // Requests a subscription.
    // The first string is the IPC label of your ICallGateProvider that FFXIV2MQTT will send it's messages to.
    // The second string is the topic you would like to subscribe to. Currently, these are automatically adjusted to the
    // client's topic path. For example, if you request "tippy/tips" you will actually be subscribed to "ffxiv/tippy/tips".
    // Return is if it was successfully subscribed.
    private readonly ICallGateProvider<string, string, bool>? providerRequestSubscription;

    private readonly Dictionary<string, ICallGateSubscriber<string, bool>> callGateSubscribers;

    private const string LabelProviderReady               = "Ffxiv2Mqtt.Ready";
    private const string LabelProviderPublishMessage      = "Ffxiv2Mqtt.Publish";
    private const string LabelProviderPublishMessageV2    = "Ffxiv2Mqtt.PublishV2";
    private const string LabelProviderRequestSubscription = "Ffxiv2Mqtt.RequestSubscription";


    public Ipc()
    {
        Service.Log.Verbose("Registering IPC Providers.");

        try {
            providerReady = Service.PluginInterface!.GetIpcProvider<bool>(LabelProviderReady);
            providerReady.RegisterFunc(Ready);
        } catch (Exception ex) {
            Service.Log.Error($"(Failed to register IPC provider for {LabelProviderReady}: {ex}");
        }

        try {
            providerPublishMessage = Service.PluginInterface!.GetIpcProvider<string, string, bool>(LabelProviderPublishMessage);
            providerPublishMessage.RegisterFunc(Publish);
        } catch (Exception ex) {
            Service.Log.Error($"Failed to register IPC provider for {LabelProviderPublishMessage}: {ex}");
        }

        try {
            providerPublishMessageV2 = Service.PluginInterface!.GetIpcProvider<Message, bool>(LabelProviderPublishMessageV2);
            providerPublishMessageV2.RegisterFunc(PublishV2);
        } catch (Exception ex) {
            Service.Log.Error($"Failed to register IPC provider for {LabelProviderPublishMessage}: {ex}");
        }

        try {
            providerRequestSubscription = Service.PluginInterface!.GetIpcProvider<string, string, bool>(LabelProviderRequestSubscription);
            providerRequestSubscription.RegisterFunc(RequestSubscription);
        } catch (Exception ex) {
            Service.Log.Error($"Failed to register IPC provider for {LabelProviderRequestSubscription}: {ex}");
        }

        callGateSubscribers = new Dictionary<string, ICallGateSubscriber<string, bool>>();

        Service.MqttManager.AddMessageReceivedHandler(IpcMessageReceivedHandler);
    }

    public void Dispose()
    {
        providerReady?.UnregisterFunc();
        providerPublishMessage?.UnregisterFunc();
        providerPublishMessageV2?.UnregisterFunc();
        providerRequestSubscription?.UnregisterFunc();
    }

    private bool Ready()
    {
        Service.Log.Information("Received IPC ready request.");
        return Service.MqttManager.IsStarted;
    }

    private bool Publish(string topic, string payload)
    {
        Service.Log.Verbose($"{LabelProviderPublishMessage} received message. Publishing {payload} to {topic}");
        return Service.MqttManager.PublishMessage(topic, payload);
    }

    private bool PublishV2(Message message)
    {
        Service.Log.Verbose($"{LabelProviderPublishMessageV2} recieved message. Publishing:/n{message.ToString()}");
        return Service.MqttManager.PublishMessage(message.Path, message.Payload, message.Retained, ToQolLevel(message.QualityOfServiceLevel));
    }

    private bool RequestSubscription(string label, string topic)
    {
        try {
            var callGateSubscriber = Service.PluginInterface.GetIpcSubscriber<string, bool>(label);
            callGateSubscribers.Add(topic, callGateSubscriber);
        } catch (Exception ex) {
            Service.Log.Error($"Failed to add a new subscription from IPC:/n{ex}");
            return false;
        }

        return true;
    }

    // This is why I need to comment things. Need to figure out how this works again so I can support wildcards in IPC.
    private Task IpcMessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic   = e.ApplicationMessage.Topic;
        var payload = e.ApplicationMessage.ConvertPayloadToString();

        callGateSubscribers[topic].InvokeFunc(payload);

        return Task.CompletedTask;
    }

    private MqttQualityOfServiceLevel ToQolLevel(QualityOfService qos)
    {
        return qos switch
        {
            QualityOfService.AtMostOnce  => MqttQualityOfServiceLevel.AtMostOnce,
            QualityOfService.AtLeastOnce => MqttQualityOfServiceLevel.AtLeastOnce,
            QualityOfService.ExactlyOnce => MqttQualityOfServiceLevel.ExactlyOnce,
            _                            => throw new ArgumentOutOfRangeException($"{qos} out of range of MqttQualityOfServiceLevel"),
        };
    }

    // Copy these into your plugin for use with IPC.
    public struct Message
    {
        public string           Path                  { get; set; }
        public string           Payload               { get; set; }
        public bool             Retained              { get; set; } = false;
        public QualityOfService QualityOfServiceLevel { get; set; } = QualityOfService.AtMostOnce;

        public Message(string path, string payload)
        {
            Path    = path;
            Payload = payload;
        }
    }

    public enum QualityOfService
    {
        AtMostOnce,
        AtLeastOnce,
        ExactlyOnce,
    }
}
