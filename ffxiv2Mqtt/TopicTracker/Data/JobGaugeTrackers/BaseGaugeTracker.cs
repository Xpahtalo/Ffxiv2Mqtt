using Dalamud.Logging;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class BaseGaugeTracker:BaseTopicTracker, IConfigurable
    {
        protected int synceTimer;
        internal BaseGaugeTracker(MqttManager m) : base(m) { }

        public virtual void Configure(Configuration configuration)
        {
            PluginLog.Verbose($"Configuring {this.GetType().Name}");
            this.synceTimer = configuration.Interval;
        }
    }
}
