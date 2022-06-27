using ImGuiNET;
using System;
using System.Numerics;
using Ffxiv2Mqtt.TopicTracker;

namespace Ffxiv2Mqtt
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;
        private MqttManager mqttManager;
        private TopicManager trackerManager;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        public PluginUI(Configuration configuration, MqttManager mqttManager, TopicManager trackerManager)
        {
            this.configuration = configuration;
            this.mqttManager = mqttManager;
            this.trackerManager = trackerManager;
        }

        public void Draw()
        {
            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            if (ImGui.Begin("Status", ref this.visible,
                ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = !SettingsVisible;
                }

                if (ImGui.Button("Connect"))
                {
                    mqttManager.ConnectToBroker();
                }

                if (ImGui.Button("Disconnect"))
                {
                    mqttManager.DisconnectFromBroker();
                }

                ImGui.Spacing();

                ImGui.Text("IsStarted: " + mqttManager.IsStarted);
                ImGui.Text("Connected: " + mqttManager.IsConnected);
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            if (ImGui.Begin("Config", ref this.settingsVisible,
                ImGuiWindowFlags.AlwaysAutoResize))
            {
                string clientId = this.configuration.ClientId;
                if (ImGui.InputText("Client ID", ref clientId, 256))
                    this.configuration.ClientId = clientId;

                bool includeClientId = this.configuration.IncludeClientId;
                if (ImGui.Checkbox("Include Client ID in topic?", ref includeClientId))
                    this.configuration.IncludeClientId = includeClientId;
                HelpMarker("This is useful if you have multiple computers connected to the same broker, so you can differentiate between them. Otherwise, leave it off.");

                string user = this.configuration.User;
                if (ImGui.InputText("User", ref user, 256))
                    this.configuration.User = user;

                string password = this.configuration.Password;
                if (ImGui.InputText("Password", ref password, 256, ImGuiInputTextFlags.Password))
                    this.configuration.Password = password;
                HelpMarker("Password is stored in plaintext. It is not secure, so please use a unique password for your user.");


                string brokerAddress = this.configuration.BrokerAddress;
                if (ImGui.InputText("Broker Address", ref brokerAddress, 2000))
                    this.configuration.BrokerAddress = brokerAddress;

                int brokerPort = this.configuration.BrokerPort;
                if (ImGui.InputInt("Broker Port", ref brokerPort))
                    this.configuration.BrokerPort = brokerPort;

                string baseTopic = this.configuration.BaseTopic;
                if (ImGui.InputText("Base Topic", ref baseTopic, 256))
                    this.configuration.BaseTopic = baseTopic;

                var fullTopic = baseTopic;
                if (includeClientId)
                    fullTopic += "/" + clientId;

                ImGui.Text($"All topics will be preceded by: {fullTopic}");

                bool connectAtStartup = this.configuration.ConnectAtStartup;
                if (ImGui.Checkbox("Connect at startup?", ref connectAtStartup))
                    this.configuration.ConnectAtStartup = connectAtStartup;

                int interval = this.configuration.Interval;
                if (ImGui.InputInt("Sync Interval", ref interval))
                    this.configuration.Interval = interval;
                HelpMarker("This is used to send messages multiple times as timers tick. 1000 is one second. Set to -1 to disable.");

                if (ImGui.Button("Save"))
                    trackerManager.Configure(configuration);
                configuration.Save();
            }
            ImGui.End();
        }

        public void Dispose()
        {
        }

        public static void HelpMarker(string text)
        {
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(text);
                ImGui.EndTooltip();
            }
        }
    }
}
