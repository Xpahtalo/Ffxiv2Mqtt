using System.Numerics;
using System.Text.Json;

namespace Ffxiv2Mqtt.Topics;

internal abstract class Topic
{
    protected abstract string TopicPath { get; }
    protected abstract bool   Retained  { get; }

    // Serializes the object into a JSON payload and publishes it to the topic.
    protected void Publish(object o) { Publish(TopicPath, JsonSerializer.Serialize(o)); }

    // Publish the payload to the TopicPath 
    protected void Publish(string payload) { Publish(TopicPath, payload); }

    // Publish the payload to the given topicPath.
    // Should only be used if the topic needs to be customized separately from TopicPath.
    protected void Publish(string topicPath, string payload)
    {
#if DEBUG
        Service.Log.Debug($"{GetType().Name}: {topicPath}: {payload}");
#endif
        Service.MqttManager.PublishMessage(topicPath, payload, Retained);
    }

    private protected static void TestValue<T>(T current, ref T previous, ref bool updated)
    {
        if (current is null || current.Equals(previous))
            return;

        previous = current;
        updated  = true;
    }

    private protected static void TestCountUp<T>(T current, ref T previous, T interval, ref bool updated)
        where T : INumber<T>
    {
        var reachedZero      = previous == T.Zero && current != T.Zero;
        var noLongerZero     = previous != T.Zero && current == T.Zero;
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
            if (interval >= T.Zero) {
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

    private protected static void TestCountDown<T>(T current, ref T previous, T interval, ref bool updated)
        where T : INumber<T>
    {
        var reachedZero      = previous == T.Zero && current != T.Zero;
        var noLongerZero     = previous != T.Zero && current == T.Zero;
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
            if (interval >= T.Zero) {
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
