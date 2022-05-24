namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal abstract class BaseGaugeTracker
    {
        internal MqttManager mqttManager;

        internal BaseGaugeTracker(MqttManager mqttManager)
        {
            this.mqttManager = mqttManager;
        }


        internal static bool CheckCountUpTimer(short previous, short current, short interval)
        {
            if ((previous == 0) && current != 0) return true;

            if (current < previous) return true;

            if (current - previous >= interval) return true;

            if ((previous != 0) && (current == 0)) return true;

            return false;
        }


        internal static bool CheckCountDownTimer(ushort previous, ushort current, ushort interval)
        {
            if ((previous == 0) && current != 0) return true;

            if (current > previous) return true;

            if (previous - current >= interval) return true;

            if ((previous != 0) && (current == 0)) return true;

            return false;
        }

        internal static bool CheckCountDownTimer(short previous, short current, short interval)
        {
            if ((previous == 0) && current != 0) return true;

            if (current > previous) return true;

            if (previous - current >= interval) return true;

            if ((previous != 0) && (current == 0)) return true;

            return false;
        }
    }
}
