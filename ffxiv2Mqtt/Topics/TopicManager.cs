using System;
using System.Collections.Generic;
using Dalamud.Logging;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics
{
    internal class TopicManager : IDisposable
    {
        private List<Topic>         topics;
        private List<IUpdatable>    updatables;
        private List<ICleanable>    cleanables;
        private List<IConfigurable> configurables;

        internal TopicManager(MqttManager m, Configuration configuration)
        {
            PluginLog.Verbose($"Creating {this.GetType().Name}");
            this.topics   = new List<Topic>();
            updatables    = new List<IUpdatable>();
            cleanables    = new List<ICleanable>();
            configurables = new List<IConfigurable>();

            Configure(configuration);
            PluginLog.Verbose($"{this.GetType().Name} created");
        }


        internal void AddTopic(Topic topic)
        {
            try {
                topics.Add(topic);
                if (topic is IUpdatable)
                    updatables.Add((IUpdatable)topic);
                if (topic is ICleanable)
                    cleanables.Add((ICleanable)topic);
                if (topic is IConfigurable)
                    configurables.Add((IConfigurable)topic);
            } catch (System.NullReferenceException) {
                PluginLog.Error("Tried to add null topic");
            }
        }

        internal void Update()
        {
            foreach (IUpdatable updatable in updatables) {
                updatable.Update();
            }
        }

        internal void Clean()
        {
            foreach (ICleanable cleanable in cleanables) {
                cleanable.Cleanup();
            }
        }

        internal void Configure(Configuration configuration)
        {
            foreach (IConfigurable configurable in configurables) {
                configurable.Configure(configuration);
            }
        }

        public void Dispose()
        {
            foreach (Topic topic in topics) {
                (topic as IDisposable)?.Dispose();
            }
        }
    }
}
