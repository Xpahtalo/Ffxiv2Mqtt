using Dalamud.Logging;
using Ffxiv2Mqtt.TopicTracker.Data;
using Ffxiv2Mqtt.TopicTracker.Events;
using Ffxiv2Mqtt.TopicTracker.Interfaces;
using System.Collections.Generic;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class TopicManager
    {
        private List<Topic> allTrackers;
        private List<IUpdatable> updatables;
        private List<ICleanable> cleanables;
        private List<IConfigurable> configurables;

        internal TopicManager(MqttManager m, Configuration configuration)
        {
            PluginLog.Verbose("Creating TrackerManager");
            allTrackers = new List<Topic>();
            updatables = new List<IUpdatable>();
            cleanables = new List<ICleanable>();
            configurables = new List<IConfigurable>();

            var trackers = new List<Topic>
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
            };

            foreach (var tracker in trackers)
                AddTracker(tracker);

            Configure(configuration);
            PluginLog.Verbose("TrackerManager created");
        }


        internal void AddTracker(Topic tracker)
        {
            try
            {
                allTrackers.Add(tracker);
                if (tracker is IUpdatable)
                    updatables.Add((IUpdatable)tracker);
                if (tracker is ICleanable)
                    cleanables.Add((ICleanable)tracker);
                if (tracker is IConfigurable)
                    configurables.Add((IConfigurable)tracker);
            }
            catch (System.NullReferenceException)
            {
                PluginLog.Error("Tried to add null tracker");
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
    }
}
