namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal abstract class BaseGaugeTracker
    {
        internal MqttManager mqttManager;

        internal BaseGaugeTracker(MqttManager mqttManager)
        {
            this.mqttManager = mqttManager;
        }
    }
}
