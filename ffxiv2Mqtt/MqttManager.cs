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
                    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                    .WithClientId(configuration.ClientId)
                    .WithTcpServer(configuration.BrokerAddress, configuration.BrokerPort)
                    .WithCredentials(configuration.User, configuration.Password)
                    .Build())
                .Build();

            mqttClient.StartAsync(options);
        }

        public string BuildTopic(string topic)
        {
            var sb = new StringBuilder(100);
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

        public void PublishRetainedMessage(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(BuildTopic(topic))
                .WithPayload(payload)
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build();

            mqttClient.PublishAsync(message);
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
                PublishRetainedMessage(topic, current.ToString());
            else
                PublishMessage(topic, current.ToString());
            previous = current;
        }

        public void Dispose()
        {
            this.DisconnectFromBroker();
            mqttClient.Dispose();
        }
    }
}
