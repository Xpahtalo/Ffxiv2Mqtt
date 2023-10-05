using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using Ffxiv2Mqtt.Enums;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace Ffxiv2Mqtt.Services;

public class MqttManager
{
    private readonly IManagedMqttClient mqttClient;
    private readonly Configuration      configuration;

    public readonly HashSet<string> CurrentSubscriptions;

    public bool IsConnected => mqttClient.IsConnected;

    public bool IsStarted => mqttClient.IsStarted;


    public MqttManager(Configuration configuration)
    {
        Service.Log.Information("Initializing MQTTManager");

        this.configuration = configuration;

        var mqttFactory = new MqttFactory();
        mqttClient = mqttFactory.CreateManagedMqttClient();

        mqttClient.ConnectedAsync        += LogConnectedAsync;
        mqttClient.ConnectingFailedAsync += LogConnectingFailedAsync;
        mqttClient.DisconnectedAsync     += LogDisconnectedAsync;

        AddMessageReceivedHandler(ConfiguredMessageReceivedHandler);

        CurrentSubscriptions = new HashSet<string>();
        ConfigureSubscribedTopics();

        Service.Log.Information("MqttManager Initialized");
    }

    private static Task LogConnectedAsync(EventArgs e)
    {
        Service.Log.Information("Connected to MQTT broker");
        return Task.CompletedTask;
    }

    private static Task LogConnectingFailedAsync(ConnectingFailedEventArgs e)
    {
        Service.Log.Warning($"Failed to connect: {e.Exception}");
        return Task.CompletedTask;
    }

    private static Task LogDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        if (e.Reason == MqttClientDisconnectReason.NormalDisconnection)
            Service.Log.Information("Disconnected from MQTT broker.");
        else
            Service.Log.Error($"Unexpected disconnect from MQTT broker: {e.Reason}");
        return Task.CompletedTask;
    }

    private Task ConfiguredMessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
    {
        Service.Log.Information("Message received");


        var messagePattern = e.ApplicationMessage.Topic.Split('/');

        var channelList = configuration.OutputChannels.AsReadOnly();

        var channelQuery =
            from channel in channelList
            where ComparePattern(messagePattern, channel.Path)
            select channel;

        foreach (var channel in channelQuery) {
            var payload = e.ApplicationMessage.ConvertPayloadToString();

            switch (channel.ChannelType) {
                case OutputChannelType.ChatBox:
                    var chatMessage = new SeStringBuilder();
                    if (channel.IncludeTopic)
                        chatMessage.Append(e.ApplicationMessage.Topic)
                                   .Append(channel.Delimiter);

                    chatMessage.AddText(payload);
                    Service.ChatGui.Print(chatMessage.Build());
                    break;
                case OutputChannelType.Toast:
                    var toast = new StringBuilder();
                    if (channel.IncludeTopic)
                        toast.Append(e.ApplicationMessage.Topic)
                             .Append(channel.Delimiter);

                    toast.Append(payload);
                    Service.ToastGui.ShowNormal(toast.ToString());
                    break;
                case OutputChannelType.Disabled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{channel} is out of range.");
            }
        }

        return Task.CompletedTask;
    }

    public static bool ComparePattern(IReadOnlyList<string> pattern, string pattern2)
    {
        var compare = pattern2.Split('/');
        for (var i = 0; i < pattern.Count; i++) {
            if (i > compare.Length) return false;

            if (pattern[i] == compare[i]) continue;

            switch (compare[i]) {
                case "#":
                    return true;
                case "+":
                    continue;
                default:
                    return false;
            }
        }

        return true;
    }


    public void ConfigureSubscribedTopics()
    {
        var topicsToAdd =
            from topic in configuration.OutputChannels
            where topic.ChannelType != OutputChannelType.Disabled
            where !CurrentSubscriptions.Contains(topic.Path)
            select topic;

        foreach (var topic in topicsToAdd) {
            mqttClient.SubscribeAsync(topic.Path);
            CurrentSubscriptions.Add(topic.Path);
        }

        var topicsToRemove =
            from topic in CurrentSubscriptions
            where configuration.OutputChannels.All(s => s.Path != topic)
            select topic;

        foreach (var topic in topicsToRemove) {
            mqttClient.UnsubscribeAsync(topic);
            CurrentSubscriptions.Remove(topic);
        }
    }

    public void ConnectToBroker()
    {
        if (configuration.BrokerAddress != string.Empty) {
            Service.Log.Information("Connecting to MQTT broker...");

            var options = new ManagedMqttClientOptionsBuilder()
                         .WithClientOptions(new MqttClientOptionsBuilder()
                                           .WithProtocolVersion(MqttProtocolVersion.V500)
                                           .WithClientId(configuration.ClientId)
                                           .WithTcpServer(configuration.BrokerAddress, configuration.BrokerPort)
                                           .WithCredentials(configuration.User, configuration.Password)
                                           .WithWillTopic(BuildTopic("connected"))
                                           .WithWillPayload("false")
                                           .WithWillRetain()
                                           .Build())
                         .Build();

            mqttClient.StartAsync(options);
            mqttClient.EnqueueAsync(ConnectedMessage());
        } else {
            Service.Log.Warning("No broker address has been set. Will not attempt to connect.");
        }
    }

    public void DisconnectFromBroker()
    {
        mqttClient.EnqueueAsync(DisconnectedMessage());
        mqttClient.StopAsync();
    }

    public void AddMessageReceivedHandler(Func<MqttApplicationMessageReceivedEventArgs, Task> handler)
    {
        try {
            mqttClient.ApplicationMessageReceivedAsync += handler;
        } catch (Exception ex) {
            Service.Log.Error($"Failed to add MessageReceivedHandler:/n{ex}");
        }
    }

    public bool PublishMessage(string topic, string payload, bool retain = false)
    {
        var qos = retain ? MqttQualityOfServiceLevel.AtLeastOnce : MqttQualityOfServiceLevel.ExactlyOnce;
        return PublishMessage(topic, payload, retain, qos);
    }

    public bool PublishMessage(string topic, string payload, bool retain, MqttQualityOfServiceLevel qos)
    {
        if (IsConnected == false) return false;
        var messageBuilder = new MqttApplicationMessageBuilder()
                            .WithTopic(BuildTopic(topic))
                            .WithPayload(payload)
                            .WithQualityOfServiceLevel(qos);

        if (retain)
            messageBuilder.WithRetainFlag();

        var message = messageBuilder.Build();

        try {
            mqttClient.EnqueueAsync(message);
        } catch (ArgumentNullException e) {
            Service.Log.Error($"Failed to publish message: {e.Message}");
            return false;
        }

        return true;
    }

    private string BuildTopic(string topic)
    {
        var sb = new StringBuilder(100);
        sb.Append(configuration.BaseTopic);
        sb.Append('/');
        if (configuration.IncludeClientId) {
            sb.Append(configuration.ClientId);
            sb.Append('/');
        }

        sb.Append(topic);

        return sb.ToString();
    }

    private MqttApplicationMessage ConnectedMessage()
    {
        return new MqttApplicationMessageBuilder()
              .WithTopic(BuildTopic("connected"))
              .WithPayload("true")
              .WithRetainFlag()
              .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
              .Build();
    }

    private MqttApplicationMessage DisconnectedMessage()
    {
        return new MqttApplicationMessageBuilder()
              .WithTopic(BuildTopic("connected"))
              .WithPayload("false")
              .WithRetainFlag()
              .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
              .Build();
    }

    public void Dispose()
    {
        if (IsConnected) DisconnectFromBroker();

        mqttClient.ConnectedAsync        -= LogConnectedAsync;
        mqttClient.ConnectingFailedAsync -= LogConnectingFailedAsync;
        mqttClient.DisconnectedAsync     -= LogDisconnectedAsync;

        mqttClient.Dispose();
    }
}
