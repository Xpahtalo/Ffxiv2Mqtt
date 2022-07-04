using System;

namespace Ffxiv2Mqtt.Topic.Events
{
    internal sealed class ContentFinderTopic : Topic, IDisposable
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

        public void Dispose()
        {
            DalamudServices.ClientState.CfPop -= CfPop;
        }
    }
}
