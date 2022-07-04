using System;
using System.Collections.Generic;
using Dalamud.Logging;
using Ffxiv2Mqtt.Topic.Data;
using Ffxiv2Mqtt.Topic.Events;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic
{
    internal class TopicManager : IDisposable
    {
        private List<Topic> topics;
        private List<IUpdatable> updatables;
        private List<ICleanable> cleanables;
        private List<IConfigurable> configurables;

        internal TopicManager(MqttManager m, Configuration configuration)
        {
            PluginLog.Verbose($"Creating {this.GetType().Name}");
            this.topics = new List<Topic>();
            updatables = new List<IUpdatable>();
            cleanables = new List<ICleanable>();
            configurables = new List<IConfigurable>();

            var topics = new List<Topic>
            {
                new PlayerInfoTopic(m),
                new PlayerCombatStatsTopic(m),
                new PlayerGathererStatsTopic(m),
                new PlayerCrafterStatsTopic(m),
                new TerritoryTopic(m),
                new ConditionsTopic(m),
                new LoginTopic(m),
                new ContentFinderTopic(m),
                new AstrologianGaugeTopic(m),
                new BardGaugeTopic(m),
                new BlackMageJobGuageTopic(m),
                new DancerGaugeTopic(m),
                new DarkKnightGaugeTopic(m),
                new DragoonGaugeTopic(m),
                new GunbreakerGaugeTopic(m),
                new MachinistGaugeTopic(m),
                new MonkGaugeTopic(m),
                new NinjaGaugeTopic(m),
                new PaladinGaugeTopic(m),
                new ReaperGaugeTopic(m),
                new RedMageGaugeTopic(m),
                new SageGaugeTopic(m),
                new ScholarGaugeTopic(m),
                new SamuraiGaugeTopic(m),
                new SummonerGaugeTopic(m),
                new WarriorGaugeTopic(m),
                new WhiteMageGaugeTopic(m),
                new WorldInfoTopic(m),
                new PlayerStatusesTopic(m),
                new PlayerCast(m),
            };

            foreach (var topic in topics)
                AddTopic(topic);

            Configure(configuration);
            PluginLog.Verbose($"{this.GetType().Name} created");
        }


        internal void AddTopic(Topic topic)
        {
            try
            {
                topics.Add(topic);
                if (topic is IUpdatable)
                    updatables.Add((IUpdatable)topic);
                if (topic is ICleanable)
                    cleanables.Add((ICleanable)topic);
                if (topic is IConfigurable)
                    configurables.Add((IConfigurable)topic);
            }
            catch (System.NullReferenceException)
            {
                PluginLog.Error("Tried to add null topic");
            }
        }
        
        internal void Update()
        {
            foreach (IUpdatable updatable in updatables)
            {
                updatable.Update();
            }
        }

        internal void Clean()
        {
            foreach (ICleanable cleanable in cleanables)
            {
                cleanable.Cleanup();
            }
        }
        
        internal void Configure(Configuration configuration)
        {
            foreach(IConfigurable configurable in configurables)
            {
                configurable.Configure(configuration);
            }
        }

        public void Dispose()
        {
            foreach(Topic topic in topics)
            {
                (topic as IDisposable)?.Dispose();
            }
        }
    }
}
