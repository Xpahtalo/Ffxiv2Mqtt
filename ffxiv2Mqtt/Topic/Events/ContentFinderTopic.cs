using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Events
{
    internal class ContentFinderTopic : Topic, ICleanable
    {
        internal ContentFinderTopic(MqttManager m) : base(m)
        {
            topic = "Event/ContentFinder";
            DalamudServices.ClientState.CfPop += CfPop;
        }

        private void CfPop(object? s, Lumina.Excel.GeneratedSheets.ContentFinderCondition e)
        {
            mqttManager.PublishMessage(topic, "Pop");
        }

        public void Cleanup()
        {
            DalamudServices.ClientState.CfPop -= CfPop;
        }
    }
}
