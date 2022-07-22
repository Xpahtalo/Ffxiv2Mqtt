using System;
using System.Numerics;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics;
using ImGuiNET;

namespace Ffxiv2Mqtt;

// It is good to have this be disposable in general, in case you ever need it
// to do any cleanup
internal class PluginUI : IDisposable
{
    private Configuration configuration;
    private readonly MqttManager   mqttManager;
    private readonly TopicManager  topicManager;

    // this extra bool exists for ImGui, since you can't ref a property
    private bool visible;

    private bool settingsVisible;

    public bool Visible
    {
        get => visible;
        set => visible = value;
    }

    public bool SettingsVisible
    {
        get => settingsVisible;
        set => settingsVisible = value;
    }

    public PluginUI(Configuration configuration, MqttManager mqttManager, TopicManager topicManager)
    {
        this.configuration = configuration;
        this.mqttManager   = mqttManager;
        this.topicManager  = topicManager;
    }

    public void Draw()
    {
        DrawMainWindow();
        DrawSettingsWindow();
    }

    private void DrawMainWindow()
    {
        if (!Visible) return;

        if (ImGui.Begin("Status", ref visible,
                        ImGuiWindowFlags.AlwaysAutoResize)) {
            if (ImGui.Button("Show Settings")) SettingsVisible = !SettingsVisible;

            if (ImGui.Button("Connect")) mqttManager.ConnectToBroker();

            if (ImGui.Button("Disconnect")) mqttManager.DisconnectFromBroker();

            ImGui.Spacing();

            ImGui.Text("IsStarted: " + mqttManager.IsStarted);
            ImGui.Text("Connected: " + mqttManager.IsConnected);
        }

        ImGui.End();
    }

    private void DrawSettingsWindow()
    {
        if (!SettingsVisible) return;

        if (ImGui.Begin("Config", ref settingsVisible, ImGuiWindowFlags.AlwaysAutoResize)) {
            var clientId = configuration.ClientId;
            if (ImGui.InputText("Client ID", ref clientId, 256)) {
                configuration.ClientId = clientId;
            }

            var includeClientId = configuration.IncludeClientId;
            if (ImGui.Checkbox("Include Client ID in topic?", ref includeClientId)) {
                configuration.IncludeClientId = includeClientId;
            }
            HelpMarker("This is useful if you have multiple computers connected to the same broker, so you can differentiate between them. Otherwise, leave it off.");

            var user = configuration.User;
            if (ImGui.InputText("User", ref user, 256)) {
                configuration.User = user;
            }

            var password = configuration.Password;
            if (ImGui.InputText("Password", ref password, 256, ImGuiInputTextFlags.Password)) {
                configuration.Password = password;
            }
            ColoredMarker(new Vector4(1, 0, 0, 1),
                          "(!)",
                          "Password is stored in plaintext. It is not secure, so please use a unique password for your user.");


            var brokerAddress = configuration.BrokerAddress;
            if (ImGui.InputText("Broker Address", ref brokerAddress, 2000)) {
                configuration.BrokerAddress = brokerAddress;
            }

            var brokerPort = configuration.BrokerPort;
            if (ImGui.InputInt("Broker Port", ref brokerPort)) {
                configuration.BrokerPort = brokerPort;
            }

            var baseTopic = configuration.BaseTopic;
            if (ImGui.InputText("Base Topic", ref baseTopic, 256)) {
                configuration.BaseTopic = baseTopic;
            }

            var fullTopic = baseTopic;
            if (includeClientId) {
                fullTopic += "/" + clientId;
            }
            ImGui.Text($"All topics will be preceded by: {fullTopic}");

            var connectAtStartup = configuration.ConnectAtStartup;
            if (ImGui.Checkbox("Connect at startup?", ref connectAtStartup)) {
                configuration.ConnectAtStartup = connectAtStartup;
            }

            var interval = configuration.Interval;
            if (ImGui.InputInt("Sync Interval", ref interval)) {
                configuration.Interval = interval;
            }
            HelpMarker("This is used to send messages multiple times as timers tick. 1000 is one second. Set to -1 to disable.");

            if (ImGui.Button("Save")) {
                topicManager.Configure(configuration);
                configuration.Save();
            }
        }

        ImGui.End();
    }

    public void Dispose() { }

    private static void HelpMarker(string text)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (!ImGui.IsItemHovered()) return;
        ImGui.BeginTooltip();
        ImGui.TextUnformatted(text);
        ImGui.EndTooltip();
    }

    private static void ColoredMarker(Vector4 color, string markerText, string tooltipText)
    {
        ImGui.SameLine();
        ImGui.TextColored(color, markerText);
        if (!ImGui.IsItemHovered()) return;
        ImGui.BeginTooltip();
        ImGui.TextUnformatted(tooltipText);
        ImGui.EndTooltip();
    }
}
