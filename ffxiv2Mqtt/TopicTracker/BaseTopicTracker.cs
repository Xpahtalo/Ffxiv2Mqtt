using Dalamud.Logging;
using Newtonsoft.Json;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class BaseTopicTracker
    {
        private protected MqttManager mqttManager;
        private protected string topic;
        private protected bool needsPublishing;

        internal BaseTopicTracker(MqttManager mqttManager)
        {
            PluginLog.Verbose($"Creating {this.GetType().Name}");
            this.mqttManager = mqttManager;
            topic = "";
        }

        private protected void TestCountUp(short current, ref short previous, short interval)
        {
            bool reachedZero = (previous == 0) && (current != 0);
            bool noLongerZero = (previous != 0) && (current == 0);
            bool wentLower = current < previous;
            bool exceededInterval = (current - previous) >= interval;

            if (reachedZero || noLongerZero || wentLower || exceededInterval)
            {
                previous = current;
                needsPublishing = true;
            }
        }
        
        private protected void TestCountDown(ushort current, ref ushort previous, ushort interval)
        {
            bool reachedZero = (previous == 0) && (current != 0);
            bool noLongerZero = (previous != 0) && (current == 0);
            bool wentHigher = current > previous;
            bool exceededInterval = (previous - current) >= interval;

            if (reachedZero || noLongerZero || wentHigher || exceededInterval)
            {
                previous = current;
                needsPublishing = true;
            }
        }
        private protected void TestCountDown(short current, ref short previous, short interval)
        {
            bool reachedZero = (previous == 0) && (current != 0);
            bool noLongerZero = (previous != 0) && (current == 0);
            bool wentHigher = current > previous;
            bool exceededInterval = (previous - current) >= interval;

            if (reachedZero || noLongerZero || wentHigher || exceededInterval)
            {
                previous = current;
                needsPublishing = true;
            }
        }
        private protected void TestCountDown(int current, ref int previous, int interval)
        {
            bool reachedZero = (previous == 0) && (current != 0);
            bool noLongerZero = (previous != 0) && (current == 0);
            bool wentHigher = current > previous;
            bool exceededInterval = (previous - current) >= interval;

            if (reachedZero || noLongerZero || wentHigher || exceededInterval)
            {
                previous = current;
                needsPublishing = true;
            }
        }
        
        private protected void TestValue<T>(T current, ref T previous)
        {
            if (!current!.Equals(previous))
            {
                previous = current;
                needsPublishing = true;
            }
        }

        
        internal virtual void PublishIfNeeded()
        {
            if (needsPublishing)
            {
                Publish();
                needsPublishing = false;
            }
        }
        internal virtual void Publish(bool retained = false)
        {
            mqttManager.PublishMessage(topic, JsonConvert.SerializeObject(this), retained);
        }
    }
}
