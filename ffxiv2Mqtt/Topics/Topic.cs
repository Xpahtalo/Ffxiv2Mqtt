using System;
using System.Text.Json;
using Dalamud.Logging;
using Dalamud.IoC;

namespace Ffxiv2Mqtt.Topics
{
    internal abstract class Topic
    {
        // ReSharper disable once MemberCanBePrivate.Global
        [PluginService] public MqttManager MqttManager { get; set; }

        private            bool   NeedsPublishing { get; set; }
        protected abstract string TopicPath       { get; }
        protected abstract bool   Retained        { get; }

        // Perform setup after injection e.g. subscribe to events.
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
            PluginLog.Debug($"{this.GetType().Name}: {topicPath}: {payload}");
#endif
            MqttManager.PublishMessage(TopicPath, payload, Retained);
        }

        // In .NET 6 and C# 11, these can be simplified down to a single method with generics using INumber.
        private protected void TestCountUp(short current, ref short previous, int interval)
        {
            bool reachedZero      = (previous == 0) && (current != 0);
            bool noLongerZero     = (previous != 0) && (current == 0);
            bool wentLower        = current              < previous;
            bool exceededInterval = (current - previous) >= interval;

            if (NeedsPublishing) // If something else has caused an update, we should also update the timer value
            {
                previous = current;
                return;
            }

            if (reachedZero || noLongerZero || wentLower) {
                previous        = current;
                NeedsPublishing = true;
                return;
            } else {
                if (interval >= 0) {
                    if (exceededInterval) {
                        previous        = current;
                        NeedsPublishing = true;
                    }
                } else // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                {
                    previous = current;
                }
            }
        }

        private protected void TestCountDown(ushort current, ref ushort previous, int interval)
        {
            bool reachedZero      = (previous == 0) && (current != 0);
            bool noLongerZero     = (previous != 0) && (current == 0);
            bool wentHigher       = current              > previous;
            bool exceededInterval = (previous - current) >= interval;

            if (NeedsPublishing) // If something else has caused an update, we should also update the timer value
            {
                previous = current;
                return;
            }

            if (reachedZero || noLongerZero || wentHigher) {
                previous        = current;
                NeedsPublishing = true;
                return;
            } else {
                if (interval >= 0) {
                    if (exceededInterval) {
                        previous        = current;
                        NeedsPublishing = true;
                    }
                } else // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                {
                    previous = current;
                }
            }
        }

        private protected void TestCountDown(short current, ref short previous, int interval)
        {
            bool reachedZero      = (previous == 0) && (current != 0);
            bool noLongerZero     = (previous != 0) && (current == 0);
            bool wentHigher       = current              > previous;
            bool exceededInterval = (previous - current) >= interval;

            if (NeedsPublishing) // If something else has caused an update, we should also update the timer value
            {
                previous = current;
                return;
            }

            if (reachedZero || noLongerZero || wentHigher) {
                previous        = current;
                NeedsPublishing = true;
                return;
            } else {
                if (interval >= 0) {
                    if (exceededInterval) {
                        previous        = current;
                        NeedsPublishing = true;
                    }
                } else // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                {
                    previous = current;
                }
            }
        }

        private protected void TestCountDown(int current, ref int previous, int interval)
        {
            bool reachedZero      = (previous == 0) && (current != 0);
            bool noLongerZero     = (previous != 0) && (current == 0);
            bool wentHigher       = current              > previous;
            bool exceededInterval = (previous - current) >= interval;

            if (NeedsPublishing) // If something else has caused an update, we should also update the timer value
            {
                previous = current;
                return;
            }

            if (reachedZero || noLongerZero || wentHigher) {
                previous        = current;
                NeedsPublishing = true;
                return;
            } else {
                if (interval >= 0) {
                    if (exceededInterval) {
                        previous        = current;
                        NeedsPublishing = true;
                    }
                } else // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                {
                    previous = current;
                }
            }
        }

        private protected void TestValue<T>(T current, ref T previous, ref bool updated) where T : IEquatable<T>
        {
            if (!current.Equals(previous)) {
                previous = current;
                updated  = true;
            }
        }
    }
}
