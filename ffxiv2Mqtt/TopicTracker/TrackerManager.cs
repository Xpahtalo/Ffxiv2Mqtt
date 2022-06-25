using Dalamud.Logging;
using Ffxiv2Mqtt.TopicTracker.Data;
using Ffxiv2Mqtt.TopicTracker.Events;
using Ffxiv2Mqtt.TopicTracker.Interfaces;
using System.Collections.Generic;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class TrackerManager
    {
        private List<BaseTopicTracker> allTrackers;
        private List<IUpdatable> updatables;
        private List<ICleanable> cleanables;
        private List<IConfigurable> configurables;

        internal TrackerManager(MqttManager m)
        {
            allTrackers = new List<BaseTopicTracker>();
            updatables = new List<IUpdatable>();
            cleanables = new List<ICleanable>();
            configurables = new List<IConfigurable>();

            var trackers = new List<BaseTopicTracker>
            {
                new PlayerInfoTracker(m),
                new PlayerCombatStatsTracker(m),
                new PlayerGathererStatsTracker(m),
                new PlayerCrafterStatsTracker(m),
                new TerritoryTracker(m),
                new ConditionTracker(m),
                new LoginTracker(m),
                new CfPopTracker(m),
                new AstrologianGaugeTracker(m),
                new BardGaugeTracker(m),
                new BlackMageGuageTracker(m),
                new DancerGaugeTracker(m),
                new DarkKnightGaugeTracker(m),
                new DragoonGaugeTracker(m),
                new GunbreakerGaugeTracker(m),
                new MachinistGaugeTracker(m),
                new MonkGaugeTracker(m),
                new NinjaGaugeTracker(m),
                new PaladinGaugeTracker(m),
                new ReaperGaugeTracker(m),
                new RedMageGaugeTracker(m),
                new SageGaugeTracker(m),
                new ScholarGaugeTracker(m),
                new SamuraiGaugeTracker(m),
                new SummonerGaugeTracker(m),
                new WarriorGaugeTracker(m),
                new WhiteMageGaugeTracker(m),
            };

            foreach (var tracker in trackers)
                AddTracker(tracker);
        }
        
        
        internal void AddTracker(BaseTopicTracker tracker)
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
