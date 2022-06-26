using Dalamud.Logging;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class BaseGaugeTracker:BaseTopicTracker, IConfigurable
    {
        protected int syncTimer;
        internal BaseGaugeTracker(MqttManager m) : base(m) { }

        public virtual void Configure(Configuration configuration)
        {
            PluginLog.Verbose($"Configuring {this.GetType().Name}");
            
            this.syncTimer = configuration.Interval;

            PluginLog.Debug($"syncTimer: {syncTimer}");
        }
    }
}
