using System;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace Ffxiv2Mqtt;

public class MqttManager
{
    private readonly IManagedMqttClient mqttClient;
    private readonly Configuration      configuration;

    public bool IsConnected => mqttClient.IsConnected;

    public bool IsStarted => mqttClient.IsStarted;


    public MqttManager(Configuration configuration)
    {
        PluginLog.Information("Initializing MQTTManager");

        this.configuration = configuration;
        mqttClient         = new MqttFactory().CreateManagedMqttClient();
#if DEBUG
        PluginLog.Debug("Hooking ConnectedAsync");
#endif
        mqttClient.ConnectedAsync += LogConnectedAsync;
#if DEBUG
        PluginLog.Debug("Hooking ConnectingFailedAsync");
#endif
        mqttClient.ConnectingFailedAsync += LogConnectingFailedAsync;
#if DEBUG
        PluginLog.Debug("Hooking DisconnectedAsync");
#endif
        mqttClient.InternalClient.DisconnectedAsync += LogDisconnectedAsync;

        PluginLog.Information("MqttManager Initialized");
    }

    public Task LogConnectedAsync(EventArgs e)
    {
        PluginLog.Information("Connected to MQTT broker");
        return Task.CompletedTask;
    }

    public Task LogConnectingFailedAsync(ConnectingFailedEventArgs e)
    {
        PluginLog.Warning($"Failed to connect: {e.Exception}");
        return Task.CompletedTask;
    }

    public Task LogDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        if (e.Reason == MqttClientDisconnectReason.NormalDisconnection)
            PluginLog.Information("Disconnected from MQTT broker.");
        else
            PluginLog.Error($"Unexpected disconnect from MQTT broker: {e.Reason}");
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


    public void PublishMessage(string topic, string payload, bool retain = false)
    {
        var messageBuilder = new MqttApplicationMessageBuilder()
                            .WithTopic(BuildTopic(topic))
                            .WithPayload(payload);

        if (retain)
            messageBuilder.WithRetainFlag().WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce);

        var message = messageBuilder.Build();

        try {
            mqttClient.EnqueueAsync(message);
        } catch (ArgumentNullException e) {
            PluginLog.Error($"Failed to publish message: {e.Message}");
        }
    }

    public string BuildTopic(string topic)
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
