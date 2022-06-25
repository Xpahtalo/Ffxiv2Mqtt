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
#if DEBUG
            PluginLog.Debug($"Configuring {this.GetType().Name}");
#endif
            this.synceTimer = configuration.Interval;
        }
    }
}
