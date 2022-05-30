using System;
using System.Text;
using Dalamud.Plugin;
using Dalamud.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
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
            this.configuration = configuration;
            mqttClient = new MqttFactory().CreateManagedMqttClient();
        }

        public void ConnectToBroker()
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(configuration.ClientId)
                    .WithTcpServer(configuration.BrokerAddress, configuration.BrokerPort)
                    .WithCredentials(configuration.User, configuration.Password)
                    .Build())
                .Build();

            mqttClient.StartAsync(options);
        }

        public string BuildTopic(string topic)
        {
            // TODO: Actually figure out what an approriate size should be.
            var sb = new StringBuilder(50);
            sb.AppendFormat("{0}/", configuration.BaseTopic);
            if (configuration.IncludeClientId)
                sb.AppendFormat("{0}/", configuration.ClientId);
            sb.AppendFormat("{0}", topic);

            return sb.ToString();
        }

        public void DisconnectFromBroker()
        {
            mqttClient.StopAsync();
        }

        public void PublishMessage(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(BuildTopic(topic))
                .WithPayload(payload)
                .Build();

            mqttClient.PublishAsync(message);
        }

        public void PublishMessage(string topic, bool payload)
        {
            PublishMessage(topic, payload.ToString());
        }

        public void PublishMessage(string topic, int payload)
        {
            PublishMessage(topic, payload.ToString());
        }

        public void PublishPersistentMessage(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(BuildTopic(topic))
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .Build();

            mqttClient.PublishAsync(message);
        }


        public void Dispose()
        {
            this.DisconnectFromBroker();
            mqttClient.Dispose();
        }
    }
}
