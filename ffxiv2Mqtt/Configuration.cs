using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Ffxiv2Mqtt
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public string ClientId { get; set; } = "FFXIV";
        public bool IncludeClientId { get; set; } = false;
        public string BrokerAddress { get; set; } = string.Empty;
        public int BrokerPort { get; set; }
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string BaseTopic { get; set; } = "ffxiv";

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
