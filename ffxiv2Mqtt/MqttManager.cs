﻿using System;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Extensions.ManagedClient;


namespace Ffxiv2Mqtt
{
    public class MqttManager
    {
        private IManagedMqttClient mqttClient;
        private Configuration configuration;
        public bool IsConnected { get => mqttClient.IsConnected; }
        public bool IsStarted { get => mqttClient.IsStarted; }


        public MqttManager(Configuration configuration)
        {
            PluginLog.Information("Initializing MQTTManager");
           
            this.configuration = configuration;
            mqttClient = new MqttFactory().CreateManagedMqttClient();

            mqttClient.ConnectedAsync += LogConnectedAsync;
            mqttClient.ConnectingFailedAsync += LogConnectingFailedAsync;
            mqttClient.InternalClient.DisconnectedAsync += LogDisconnectedAsync;

            PluginLog.Log("MqttManager Initialized");
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
                    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                    .WithClientId(configuration.ClientId)
                    .WithTcpServer(configuration.BrokerAddress, configuration.BrokerPort)
                    .WithCredentials(configuration.User, configuration.Password)
                    .Build())
                .Build();

            mqttClient.StartAsync(options);
        }
        
        public void DisconnectFromBroker()
        {
            mqttClient.StopAsync();
        }


        public void PublishMessage(string topic, string payload, bool retain = false)
        {
            var messageBuilder = new MqttApplicationMessageBuilder()
               .WithTopic(BuildTopic(topic))
               .WithPayload(payload);

            if (retain) messageBuilder.WithRetainFlag().WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce);

            var message = messageBuilder.Build();

            try
            {
                mqttClient.EnqueueAsync(message);
            }
            catch (ArgumentNullException e)
            {
                PluginLog.Error($"Failed to publish message: {e.Message}");
            }

        }
        public string BuildTopic(string topic)
        {
            var sb = new StringBuilder(100);
            sb.Append(configuration.BaseTopic);
            sb.Append('/');
            if (configuration.IncludeClientId)
            {
                sb.Append(configuration.ClientId);
                sb.Append('/');
            }
            sb.Append(topic);

            return sb.ToString();
        }


        // Switching these to use generics would end up being more trouble than it's worth.
        internal void TestCountUp(short current, ref short previous, short interval, string topic)
        {
            if (((previous == 0) && current != 0)
               || ((previous != 0) && (current == 0))
               || (current < previous)
               || (current - previous >= interval))
                UpdateAndPublish(current, ref previous, topic, false);
        }
        internal void TestCountDown(ushort current, ref ushort previous, ushort interval, string topic)
        {
            if (((previous == 0) && current != 0)
                || ((previous != 0) && (current == 0))
                || (current > previous)
                || (previous - current >= interval))
                UpdateAndPublish(current, ref previous, topic, false);
        }
        internal void TestCountDown(short current, ref short previous, short interval, string topic)
        {
            if (((previous == 0) && current != 0)
                || ((previous != 0) && (current == 0))
                || (current > previous)
                || (previous - current >= interval))
                UpdateAndPublish(current, ref previous, topic, false);
        }
        internal void TestValue<T>(T current, ref T previous, string topic, bool retained)
        {
            if (current == null) return;
            if (!current.Equals(previous)) UpdateAndPublish(current, ref previous, topic, retained);
        }
        internal void TestValue<T>(T current, ref T previous, string topic)
        {
            if (current == null) return;
            if (!current.Equals(previous)) UpdateAndPublish(current, ref previous, topic, false);
        }
        internal void UpdateAndPublish<T>(T current, ref T previous, string topic, bool retained)
        {
            if (retained)
                PublishMessage(topic, current?.ToString() ?? "", true);
            else
                PublishMessage(topic, current?.ToString() ?? "", false);
            previous = current;
        }


        public void Dispose()
        {
            this.DisconnectFromBroker();

            mqttClient.ConnectedAsync -= LogConnectedAsync;
            mqttClient.ConnectingFailedAsync -= LogConnectingFailedAsync;
            mqttClient.InternalClient.DisconnectedAsync -= LogDisconnectedAsync;

            mqttClient.Dispose();
        }
    }
}
