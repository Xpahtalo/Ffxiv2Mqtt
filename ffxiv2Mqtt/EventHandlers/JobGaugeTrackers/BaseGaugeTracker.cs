namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal abstract class BaseGaugeTracker
    {
        internal MqttManager mqttManager;

        internal BaseGaugeTracker(MqttManager mqttManager)
        {
            this.mqttManager = mqttManager;
        }


        // Switching these to use generics would end up being more trouble than it's worth.
        internal void TestCountUp(short current, ref short previous, short interval, string topic)
        {
            if (((previous == 0) && current != 0)
               || ((previous != 0) && (current == 0))
               || (current < previous)
               || (current - previous >= interval))
                UpdateAndPublish(current, ref previous, topic);
        }
        internal void TestCountDown(ushort current, ref ushort previous, ushort interval, string topic)
        {
            if (((previous == 0) && current != 0)
                || ((previous != 0) && (current == 0))
                || (current > previous)
                || (previous - current >= interval))
                UpdateAndPublish(current, ref previous, topic);
        }
        internal void TestCountDown(short current, ref short previous, short interval, string topic)
        {
            if (((previous == 0) && current != 0)
                || ((previous != 0) && (current == 0))
                || (current > previous)
                || (previous - current >= interval))
                UpdateAndPublish(current, ref previous, topic);
        }
        
        internal void TestValue<T>(T current, ref T previous, string topic)
        {
            if (current == null) return;

            if (!current.Equals(previous)) UpdateAndPublish(current, ref previous, topic);
        }

        internal void UpdateAndPublish<T>(T current, ref T previous, string topic)
        {
            mqttManager.PublishMessage(topic, current.ToString());
            previous = current;
        }
    }
}
