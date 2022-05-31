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
        internal static bool CheckCountUpTimer(short previous, short current, short interval)
        {
            if ((previous == 0) && current != 0) return true;

            if (current < previous) return true;

            if (current - previous >= interval) return true;

            if ((previous != 0) && (current == 0)) return true;

            return false;
        }
        internal void TestCountUp(short current, ref short previous, short interval, string topic)
        {
            if (!((previous == 0) && current != 0)) return;
            if (!(current < previous)) return;
            if (!(current - previous >= interval)) return;
            if (!((previous != 0) && (current == 0))) return;
            UpdateAndPublish(current, ref previous, topic);
        }
        internal static bool CheckCountDownTimer(ushort previous, ushort current, ushort interval)
        {
            if ((previous == 0) && current != 0) return true;

            if (current > previous) return true;

            if (previous - current >= interval) return true;

            if ((previous != 0) && (current == 0)) return true;

            return false;
        }
        internal void TestCountDown(ushort current, ref ushort previous, ushort interval, string topic)
        {
            if (!((previous == 0) && current != 0)) return;
            if (!(current > previous)) return;
            if (!(previous - current >= interval)) return;
            if (!((previous != 0) && (current == 0))) return;
            UpdateAndPublish(current, ref previous, topic);
        }
        internal static bool CheckCountDownTimer(short previous, short current, short interval)
        {
            if ((previous == 0) && current != 0) return true;

            if (current > previous) return true;

            if (previous - current >= interval) return true;

            if ((previous != 0) && (current == 0)) return true;

            return false;
        }
        internal void TestCountDown(short current, ref short previous, short interval, string topic)
        {
            if (!((previous == 0) && current != 0)) return;
            if (!(current > previous)) return;
            if (!(previous - current >= interval)) return;
            if (!((previous != 0) && (current == 0))) return;
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
