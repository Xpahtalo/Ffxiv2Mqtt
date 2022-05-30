using System;
using System.Text;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace Ffxiv2Mqtt
{
    [PluginInterface]
    public class MqttManager : IDisposable
    {
        private IManagedMqttClient mqttClient;

        private bool disposed = false;

        [PluginService]
        [RequiredVersion("1.0")]
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService]
        [RequiredVersion("1.0")]
        public static Configuration Configuration { get; private set; } = null!;
        public bool IsConnected { get => mqttClient.IsConnected; }
        public bool IsStarted { get => mqttClient.IsStarted; }



        public static void Initialize(DalamudPluginInterface pluginInterface) =>
            pluginInterface.Create<MqttManager>();

        public MqttManager(DalamudPluginInterface pluginInterface)
        {
            MqttManager.Initialize(pluginInterface);
            mqttClient = new MqttFactory().CreateManagedMqttClient();
        }

        public void ConnectToBroker()
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(Configuration.ClientId)
                    .WithTcpServer(Configuration.BrokerAddress, Configuration.BrokerPort)
                    .WithCredentials(Configuration.User, Configuration.Password)
                    .Build())
                .Build();

            mqttClient.StartAsync(options);
        }

        public string BuildTopic(string topic)
        {
            // TODO: Actually figure out what an approriate size should be.
            var sb = new StringBuilder(50);
            sb.AppendFormat("{0}/", Configuration.BaseTopic);
            if (Configuration.IncludeClientId)
                sb.AppendFormat("{0}/", Configuration.ClientId);
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


        private void Dispose(bool disposing)
        {
            if (!disposed) return;
            this.DisconnectFromBroker();
            mqttClient.Dispose();
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
