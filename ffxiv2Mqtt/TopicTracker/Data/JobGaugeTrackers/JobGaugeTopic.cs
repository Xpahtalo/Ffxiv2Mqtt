using Dalamud.Logging;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
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
