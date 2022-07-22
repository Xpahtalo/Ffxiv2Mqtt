using System.Text.Json;
using Dalamud.IoC;
using Dalamud.Logging;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics;

internal abstract class Topic
{
    protected abstract string TopicPath { get; }
    protected abstract bool   Retained  { get; }

    // ReSharper disable once MemberCanBePrivate.Global
    [PluginService] public MqttManager MqttManager { get; set; }

    // Perform setup after property injection e.g. subscribe to events.
    public abstract void Initialize();

    // Serializes the object into a JSON payload and publishes it to the topic.
    protected void Publish(object o)
    {
        Publish(TopicPath, JsonSerializer.Serialize(o));
    }

    // Publish the payload to the TopicPath 
    protected void Publish(string payload)
    {
        Publish(TopicPath, payload);
    }

    // Publish the payload to the given topicPath.
    // Should only be used if the topic needs to be customized separately from TopicPath.
    protected void Publish(string topicPath, string payload)
    {
#if DEBUG
        PluginLog.Debug($"{GetType().Name}: {topicPath}: {payload}");
#endif
        MqttManager.PublishMessage(topicPath, payload, Retained);
    }

    private protected static void TestValue<T>(T current, ref T previous, ref bool updated)
    {
        if (current is null)
            return;
        if (!current.Equals(previous)) {
            previous = current;
            updated  = true;
        }
    }

    // In .NET 6 and C# 11, these can be simplified down to a single method with generics using INumber.
    private protected static void TestCountUp(short current, ref short previous, int interval, ref bool updated)
    {
        var reachedZero      = previous == 0 && current != 0;
        var noLongerZero     = previous != 0 && current == 0;
        var wentLower        = current            < previous;
        var exceededInterval = current - previous >= interval;

        // If something else has caused an update, we should also update the timer value
        if (updated) {
            previous = current;
            return;
        }

        if (reachedZero || noLongerZero || wentLower) {
            previous = current;
            updated  = true;
        } else {
            if (interval >= 0) {
                if (exceededInterval) {
                    previous = current;
                    updated  = true;
                }
            } else {
                // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                previous = current;
            }
        }
    }

    private protected static void TestCountDown(ushort current, ref ushort previous, int interval, ref bool updated)
    {
        var reachedZero      = previous == 0 && current != 0;
        var noLongerZero     = previous != 0 && current == 0;
        var wentHigher       = current            > previous;
        var exceededInterval = previous - current >= interval;

        // If something else has caused an update, we should also update the timer value
        if (updated) {
            previous = current;
            return;
        }

        if (reachedZero || noLongerZero || wentHigher) {
            previous = current;
            updated  = true;
        } else {
            if (interval >= 0) {
                if (exceededInterval) {
                    previous = current;
                    updated  = true;
                }
            } else {
                // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                previous = current;
            }
        }
    }

    private protected static void TestCountDown(short current, ref short previous, int interval, ref bool updated)
    {
        var reachedZero      = previous == 0 && current != 0;
        var noLongerZero     = previous != 0 && current == 0;
        var wentHigher       = current            > previous;
        var exceededInterval = previous - current >= interval;

        // If something else has caused an update, we should also update the timer value
        if (updated) {
            previous = current;
            return;
        }

        if (reachedZero || noLongerZero || wentHigher) {
            previous = current;
            updated  = true;
        } else {
            if (interval >= 0) {
                if (exceededInterval) {
                    previous = current;
                    updated  = true;
                }
            } else {
                // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                previous = current;
            }
        }
    }

    private protected static void TestCountDown(int current, ref int previous, int interval, ref bool updated)
    {
        var reachedZero      = previous == 0 && current != 0;
        var noLongerZero     = previous != 0 && current == 0;
        var wentHigher       = current            > previous;
        var exceededInterval = previous - current >= interval;

        // If something else has caused an update, we should also update the timer value
        if (updated) {
            previous = current;
            return;
        }

        if (reachedZero || noLongerZero || wentHigher) {
            previous = current;
            updated  = true;
        } else {
            if (interval >= 0) {
                if (exceededInterval) {
                    previous = current;
                    updated  = true;
                }
            } else {
                // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                previous = current;
            }
        }
    }
}
