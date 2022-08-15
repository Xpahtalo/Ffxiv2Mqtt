using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using Dalamud.Logging;
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

    private ChatGui  ChatGui  { get; }
    private ToastGui ToastGui { get; }

    public bool IsConnected => mqttClient.IsConnected;

    public bool IsStarted => mqttClient.IsStarted;


    public MqttManager([RequiredVersion("1.0")] Configuration configuration,
                       [RequiredVersion("1.0")] ChatGui       chatGui,
                       [RequiredVersion("1.0")] ToastGui      toastGui)
    {
        PluginLog.Information("Initializing MQTTManager");

        this.configuration = configuration;
        ChatGui            = chatGui;
        ToastGui           = toastGui;

        var mqttFactory = new MqttFactory();
        mqttClient = mqttFactory.CreateManagedMqttClient();

        mqttClient.ConnectedAsync                   += LogConnectedAsync;
        mqttClient.ConnectingFailedAsync            += LogConnectingFailedAsync;
        mqttClient.InternalClient.DisconnectedAsync += LogDisconnectedAsync;

        AddMessageReceivedHandler(ConfiguredMessageReceivedHandler);

        foreach (var topic in configuration.OutputChannels) mqttClient.SubscribeAsync(topic.Path);

        PluginLog.Information("MqttManager Initialized");
    }

    private static Task LogConnectedAsync(EventArgs e)
    {
        PluginLog.Information("Connected to MQTT broker");
        return Task.CompletedTask;
    }

    private static Task LogConnectingFailedAsync(ConnectingFailedEventArgs e)
    {
        PluginLog.Warning($"Failed to connect: {e.Exception}");
        return Task.CompletedTask;
    }

    private static Task LogDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        if (e.Reason == MqttClientDisconnectReason.NormalDisconnection)
            PluginLog.Information("Disconnected from MQTT broker.");
        else
            PluginLog.Error($"Unexpected disconnect from MQTT broker: {e.Reason}");
        return Task.CompletedTask;
    }

    private Task ConfiguredMessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
    {
        PluginLog.Information("Message received");

        var channelList = configuration.OutputChannels.AsReadOnly();

        var channelQuery =
            from channel in channelList
            where channel.Path == e.ApplicationMessage.Topic
            select channel;

        foreach (var channel in channelQuery) {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload, 0, e.ApplicationMessage.Payload.Length);

            switch (channel.ChannelType) {
                case OutputChannelType.ChatBox:
                    var chatMessage = new SeStringBuilder()
                                     .AddText(e.ApplicationMessage.Topic)
                                     .Append(" => ")
                                     .Append(payload)
                                     .Build();
                    ChatGui.Print(chatMessage);
                    break;
                case OutputChannelType.Toast:
                    var toast = new StringBuilder()
                               .Append(e.ApplicationMessage.Topic)
                               .Append(" => ")
                               .Append(payload);
                    ToastGui.ShowNormal(toast.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{channel} is out of range.");
            }
        }

        return Task.CompletedTask;
    }


    public void ConnectToBroker()
    {
        PluginLog.Information("Connecting to MQTT broker...");

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
    }

    public void DisconnectFromBroker()
    {
        mqttClient.EnqueueAsync(DisconnectedMessage());
        mqttClient.StopAsync();
    }

    public bool AddMessageReceivedHandler(Func<MqttApplicationMessageReceivedEventArgs, Task> handler)
    {
        try {
            mqttClient.ApplicationMessageReceivedAsync += handler;
        } catch (Exception ex) {
            PluginLog.Error($"Failed to add MessageReceivedHandler:/n{ex}");
            return false;
        }

        return true;
    }

    public bool PublishMessage(string topic, string payload, bool retain = false)
    {
        var qos = retain ? MqttQualityOfServiceLevel.AtLeastOnce : MqttQualityOfServiceLevel.ExactlyOnce;
        return PublishMessage(topic, payload, retain, qos);
    }

    public bool PublishMessage(string topic, string payload, bool retain, MqttQualityOfServiceLevel qos)
    {
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
            PluginLog.Error($"Failed to publish message: {e.Message}");
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
        DisconnectFromBroker();

        mqttClient.ConnectedAsync                   -= LogConnectedAsync;
        mqttClient.ConnectingFailedAsync            -= LogConnectingFailedAsync;
        mqttClient.InternalClient.DisconnectedAsync -= LogDisconnectedAsync;

        mqttClient.Dispose();
    }
}
