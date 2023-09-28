using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace Ffxiv2Mqtt;

[Serializable]
public class Configuration : IPluginConfiguration
{
    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;

    public int Version { get; set; } = 1;

    // MQTT Settings
    public string              ClientId         { get; set; } = "FFXIV";
    public bool                IncludeClientId  { get; set; } = false;
    public string              BrokerAddress    { get; set; } = string.Empty;
    public int                 BrokerPort       { get; set; } = 1883;
    public string              User             { get; set; } = string.Empty;
    public string              Password         { get; set; } = string.Empty;
    public string              BaseTopic        { get; set; } = "ffxiv";
    public bool                ConnectAtStartup { get; set; } = false;
    public List<OutputChannel> OutputChannels   { get; set; } = new();

    // Path Settings
    public int Interval { get; set; } = 5000;

    public void Initialize(DalamudPluginInterface pluginInterface) { this.pluginInterface = pluginInterface; }

    public void Save() { pluginInterface!.SavePluginConfig(this); }
}
