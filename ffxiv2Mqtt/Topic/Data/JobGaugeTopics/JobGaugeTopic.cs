using Dalamud.Logging;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class JobGaugeTopic:Topic, IConfigurable
    {
        protected int syncTimer;
        internal JobGaugeTopic(MqttManager m) : base(m) { }

        public virtual void Configure(Configuration configuration)
        {
            PluginLog.Verbose($"Configuring {this.GetType().Name}");
            
            this.syncTimer = configuration.Interval;

            PluginLog.Debug($"syncTimer: {syncTimer}");
        }
    }
}
