using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics;
using ImGuiNET;

namespace Ffxiv2Mqtt.Interface;

internal class MainWindow : Window 
{
    private readonly Configuration configuration;
    private readonly MqttManager   mqttManager;
    private readonly TopicManager  topicManager;

    public MainWindow(Configuration configuration, MqttManager mqttManager, TopicManager topicManager)
        : base("FFXIV 2 MQTT")
    {
        this.configuration = configuration;
        this.mqttManager   = mqttManager;
        this.topicManager  = topicManager;
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("TabBar")) {
            DisplayStatusTab();
            DisplayMqttSettingsTab();
            DisplaySubscriptionSettingsTab();
            ImGui.EndTabBar();
        }
    }

    #region Status
    private void DisplayStatusTab()
    {
        if (!ImGui.BeginTabItem("Status"))
            return;

        ImGui.Text("Client Started: " + mqttManager.IsStarted);
        ImGui.Text("Connection Status: " + mqttManager.IsConnected);
        
        ImGui.Spacing();

        if (mqttManager.IsConnected) {
            if (ImGui.Button("Disconnect")) {
                mqttManager.DisconnectFromBroker();
            }
        } else {
            if (ImGui.Button("Connect")) {
                mqttManager.ConnectToBroker();
            }
        }
        
        #if DEBUG
        ImGui.Separator();
        ImGui.Text("Debug");
        ImGui.Text("Current Subscriptions:");
        foreach (var subscription in mqttManager.CurrentSubscriptions) {
            ImGui.Text(subscription);
        }
        #endif

        ImGui.EndTabItem();
    }
    #endregion

    #region Settings
    private void DisplayMqttSettingsTab()
    {
        if (!ImGui.BeginTabItem("Settings"))
            return;

        var clientId                                                                = configuration.ClientId;
        if (ImGui.InputText("Client ID", ref clientId, 256)) configuration.ClientId = clientId;

        var includeClientId                                                                                   = configuration.IncludeClientId;
        if (ImGui.Checkbox("Include Client ID in topic?", ref includeClientId)) configuration.IncludeClientId = includeClientId;
        HelpMarker("This is useful if you have multiple computers connected to the same broker, so you can differentiate between them. Otherwise, leave it off.");

        var user                                                       = configuration.User;
        if (ImGui.InputText("User", ref user, 256)) configuration.User = user;

        var password                                                                                             = configuration.Password;
        if (ImGui.InputText("Password", ref password, 256, ImGuiInputTextFlags.Password)) configuration.Password = password;
        ColoredMarker(new Vector4(1, 0, 0, 1),
                      "(!)",
                      "Password is stored in plaintext. It is not secure, so please use a unique password for your user.");


        var brokerAddress                                                                           = configuration.BrokerAddress;
        if (ImGui.InputText("Broker Address", ref brokerAddress, 2000)) configuration.BrokerAddress = brokerAddress;

        var brokerPort                                                              = configuration.BrokerPort;
        if (ImGui.InputInt("Broker Port", ref brokerPort)) configuration.BrokerPort = brokerPort;

        var baseTopic                                                                 = configuration.BaseTopic;
        if (ImGui.InputText("Base Path", ref baseTopic, 256)) configuration.BaseTopic = baseTopic;

        var fullTopic                  = baseTopic;
        if (includeClientId) fullTopic += "/" + clientId;
        ImGui.Text($"All topics will be preceded by: {fullTopic}");

        var connectAtStartup                                                                            = configuration.ConnectAtStartup;
        if (ImGui.Checkbox("Connect at startup?", ref connectAtStartup)) configuration.ConnectAtStartup = connectAtStartup;

        var interval                                                              = configuration.Interval;
        if (ImGui.InputInt("Sync Interval", ref interval)) configuration.Interval = interval;
        HelpMarker("This is used to send messages multiple times as timers tick. 1000 is one second. Set to -1 to disable.");

        if (ImGui.Button("Save")) {
            topicManager.Configure(configuration);
            configuration.Save();
        }

        ImGui.EndTabItem();
    }
    #endregion

    #region Subscriptions
    private void DisplaySubscriptionSettingsTab()
    {
        if (!ImGui.BeginTabItem("Subscriptions"))
            return;

        DrawSubscriptionButtons();
        DrawSubscriptions();

        if (ImGui.Button("Save")) {
            configuration.Save();
            mqttManager.ConfigureSubscribedTopics();
        }
        
        ImGui.EndTabItem();
    }

    private void DrawSubscriptionButtons()
    {
        if (ImGui.Button("+")) {
            configuration.OutputChannels.Add(new OutputChannel
                                             {
                                                 Path        = "",
                                                 ChannelType = OutputChannelType.Disabled,
                                             });
        }
    }

    private void DrawSubscriptions()
    {
        int i = 0;
        foreach (var outputChannel in configuration.OutputChannels.ToArray()) {
            ImGui.PushID(i++);

            var path         = outputChannel.Path;
            var channelType  = outputChannel.ChannelType;
            var includeTopic = outputChannel.IncludeTopic;
            var delimiter    = outputChannel.Delimiter;


            if (ImGui.InputText($"Topic##{i}", ref path, 2000)) {
                outputChannel.Path = path;
            }

            if (ImGui.BeginCombo($"Output##{i}", channelType.ToString())) {
                foreach (var test in Enum.GetValues(typeof(OutputChannelType))) {
                    if (!ImGui.Selectable($"{test.ToString()}##{i}")) {
                        continue;
                    }

                    OutputChannelType.TryParse(test.ToString(), out channelType);
                    outputChannel.ChannelType = channelType;
                }


                ImGui.EndCombo();
            }

            if (ImGui.Checkbox($"Include Topic?##{i}", ref includeTopic)) {
                outputChannel.IncludeTopic = includeTopic;
            }

            if (ImGui.InputText($"Delimiter##{i}", ref delimiter, 10)) {
                outputChannel.Delimiter = delimiter;
            }

            if (ImGui.Button("Remove")) {
                configuration.OutputChannels.Remove(outputChannel);
            }

            ImGui.PopID();
        }
    }

    #endregion

    #region Helpers
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
    #endregion

}
