using Dalamud.Logging;
using Ffxiv2Mqtt.TopicTracker.Interfaces;
using System.Collections.Generic;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class TrackerManager
    {
        private List<BaseTopicTracker> allTrackers;
        private List<IUpdatable> updatables;
        private List<ICleanable> cleanables;

        internal TrackerManager()
        {
            allTrackers = new List<BaseTopicTracker>();
            updatables = new List<IUpdatable>();
            cleanables = new List<ICleanable>();
        }
        internal TrackerManager(List<BaseTopicTracker> trackers)
        {
            allTrackers = new List<BaseTopicTracker>();
            updatables = new List<IUpdatable>();
            cleanables = new List<ICleanable>();
            
            foreach (BaseTopicTracker tracker in trackers)
                AddTracker(tracker);
        }
        
        
        internal void AddTracker(BaseTopicTracker tracker)
        {
            try
            {
                allTrackers.Add(tracker);
                if (tracker is IUpdatable)
                {
                    updatables.Add((IUpdatable)tracker);
                }
                if (tracker is ICleanable)
                {
                    cleanables.Add((ICleanable)tracker);
                }
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
    }
}
