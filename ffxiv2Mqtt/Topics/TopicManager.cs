using System;
using System.Collections.Generic;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics;

internal class TopicManager : IDisposable
{
    private readonly List<Topic>         topics;
    private readonly List<ICleanable>    cleanables;
    private readonly List<IConfigurable> configurables;

    internal TopicManager(MqttManager m, Configuration configuration)
    {
        Service.Log.Verbose($"Creating {GetType().Name}");
        topics        = new List<Topic>();
        cleanables    = new List<ICleanable>();
        configurables = new List<IConfigurable>();

        Configure(configuration);
        Service.Log.Verbose($"{GetType().Name} created");
    }


    internal void AddTopic(Topic topic)
    {
        try {
            topics.Add(topic);
            if (topic is ICleanable)
                cleanables.Add((ICleanable)topic);
            if (topic is IConfigurable)
                configurables.Add((IConfigurable)topic);
        } catch (NullReferenceException) {
            Service.Log.Error("Tried to add null topic");
        }
    }

    internal void Clean()
    {
        foreach (var cleanable in cleanables) cleanable.Cleanup();
    }

    internal void Configure(Configuration configuration)
    {
        foreach (var configurable in configurables) configurable.Configure();
    }

    public void Dispose()
    {
        foreach (var topic in topics) (topic as IDisposable)?.Dispose();
    }
}
