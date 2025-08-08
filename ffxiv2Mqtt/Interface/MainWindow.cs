using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Topics;

namespace Ffxiv2Mqtt.Interface;

internal class MainWindow : Window
{
    private readonly Configuration configuration;
    private readonly TopicManager  topicManager;

    public MainWindow(Configuration configuration, TopicManager topicManager)
        : base("FFXIV2MQTT")
    {
        this.configuration = configuration;
        this.topicManager  = topicManager;
    }

    public override void Draw()
    {
        using var tabBar = ImRaii.TabBar("TabBar", ImGuiTabBarFlags.None);

        if (!tabBar)
            return;

        DisplayStatusTab();
        DisplayMqttSettingsTab();
        DisplaySubscriptionSettingsTab();
    }

#region Status

    private void DisplayStatusTab()
    {
        using var statusTab = ImRaii.TabItem("Status");
        if (!statusTab)
            return;

        if (configuration.BrokerAddress == string.Empty ||
            configuration.User          == string.Empty ||
            configuration.Password      == string.Empty) {
            ImGui.TextColored(ImGuiColors.HealerGreen, "Please configure your client settings to connect.");
        } else {
            ImGui.Text("Client Started: "    + Service.MqttManager.IsStarted);
            ImGui.Text("Connection Status: " + Service.MqttManager.IsConnected);

            ImGui.Spacing();

            if (Service.MqttManager.IsStarted) {
                if (ImGui.Button("Disconnect")) Service.MqttManager.DisconnectFromBroker();
            } else {
                if (ImGui.Button("Connect")) Service.MqttManager.ConnectToBroker();
            }
        }

#if DEBUG
        ImGui.Separator();
        ImGui.Text("Debug");
        ImGui.Text("Current Subscriptions:");
        foreach (var subscription in Service.MqttManager.CurrentSubscriptions) ImGui.Text(subscription);
#endif
    }

#endregion

#region Settings

    private void DisplayMqttSettingsTab()
    {
        using var settingsTab = ImRaii.TabItem("Settings");
        if (!settingsTab)
            return;

        var clientId                                                                = configuration.ClientId;
        if (ImGui.InputText("Client ID", ref clientId, 256)) configuration.ClientId = clientId;

        var includeClientId = configuration.IncludeClientId;
        if (ImGui.Checkbox("Include Client ID in topic?", ref includeClientId))
            configuration.IncludeClientId = includeClientId;
        HelpMarker("This is useful if you have multiple computers connected to the same broker, so you can differentiate between them. Otherwise, leave it off.");

        var user                                                       = configuration.User;
        if (ImGui.InputText("User", ref user, 256)) configuration.User = user;

        var password = configuration.Password;
        if (ImGui.InputText("Password", ref password, 256, ImGuiInputTextFlags.Password))
            configuration.Password = password;
        ColoredMarker(new Vector4(1, 0, 0, 1),
                      "(!)",
                      "Password is stored in plaintext. It is not secure, so please use a unique password for your user.");


        var brokerAddress                                                                           = configuration.BrokerAddress;
        if (ImGui.InputText("Broker Address", ref brokerAddress, 2000)) configuration.BrokerAddress = brokerAddress;

        var brokerPort                                                              = configuration.BrokerPort;
        if (ImGui.InputInt("Broker Port", ref brokerPort)) configuration.BrokerPort = brokerPort;

        var baseTopic                                                                  = configuration.BaseTopic;
        if (ImGui.InputText("Base Topic", ref baseTopic, 256)) configuration.BaseTopic = baseTopic;

        var fullTopic                  = baseTopic;
        if (includeClientId) fullTopic += "/" + clientId;
        ImGui.Text($"All topics will be preceded by: {fullTopic}/");

        var connectAtStartup = configuration.ConnectAtStartup;
        if (ImGui.Checkbox("Connect at startup?", ref connectAtStartup))
            configuration.ConnectAtStartup = connectAtStartup;

        var interval                                                              = configuration.Interval;
        if (ImGui.InputInt("Sync Interval", ref interval)) configuration.Interval = interval;
        HelpMarker("This is used to send messages multiple times as timers tick. 1000 is one second. Set to -1 to disable.");

        if (ImGui.Button("Save")) {
            topicManager.Configure(configuration);
            configuration.Save();
        }
    }

#endregion

#region Subscriptions

    private void DisplaySubscriptionSettingsTab()
    {
        using var subscriptionsBar = ImRaii.TabItem("Subscriptions");
        if (!subscriptionsBar)
            return;

        DrawAddSubscriptionButton();
        DrawSubscriptions();

        if (ImGui.Button("Save")) {
            configuration.Save();
            Service.MqttManager.ConfigureSubscribedTopics();
        }
    }

    private void DrawAddSubscriptionButton()
    {
        if (ImGui.Button("Add New Subscription"))
            configuration.OutputChannels.Add(new OutputChannel
                                             {
                                                 Path        = "",
                                                 ChannelType = OutputChannelType.Disabled,
                                             });
    }

    private void DrawSubscriptions()
    {
        var i = 0;
        foreach (var outputChannel in configuration.OutputChannels.ToArray()) {
            using var id = new ImRaii.Id().Push(i++);

            var path         = outputChannel.Path;
            var channelType  = outputChannel.ChannelType;
            var includeTopic = outputChannel.IncludeTopic;
            var delimiter    = outputChannel.Delimiter;

            if (ImGui.InputText("Topic", ref path, 2000)) outputChannel.Path = path;

            using (var outputCombo = ImRaii.Combo("Output", $"{channelType}")) {
                if (outputCombo)
                    foreach (var outputChannelType in Enum.GetValues(typeof(OutputChannelType))) {
                        if (!ImGui.Selectable($"{outputChannelType}"))
                            continue;

                        Enum.TryParse(outputChannelType.ToString(), out channelType);
                        outputChannel.ChannelType = channelType;
                    }
            }

            if (ImGui.Checkbox("Include Topic?", ref includeTopic)) outputChannel.IncludeTopic = includeTopic;

            if (ImGui.InputText("Delimiter", ref delimiter, 10)) outputChannel.Delimiter = delimiter;

            if (ImGui.Button("Remove")) configuration.OutputChannels.Remove(outputChannel);
        }
    }

#endregion

#region Helpers

    private static void HelpMarker(string text)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (!ImGui.IsItemHovered())
            return;
        ImGui.BeginTooltip();
        ImGui.Text(text);
        ImGui.EndTooltip();
    }

    private static void ColoredMarker(Vector4 color, string markerText, string tooltipText)
    {
        ImGui.SameLine();
        ImGui.TextColored(color, markerText);
        if (!ImGui.IsItemHovered())
            return;
        ImGui.BeginTooltip();
        ImGui.Text(tooltipText);
        ImGui.EndTooltip();
    }

#endregion
}
