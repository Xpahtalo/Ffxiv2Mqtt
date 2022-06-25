using Dalamud.Game.ClientState.Conditions;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Events
{
    internal class ConditionTracker : BaseTopicTracker, ICleanable
    {
        internal ConditionTracker(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Conditions";

            Dalamud.Conditions.ConditionChange += ConditionChange;
        }

        private void ConditionChange(ConditionFlag flag, bool value)
        {
            mqttManager.PublishMessage($"{topic}/{flag}", value.ToString());
        }

        public void Cleanup()
        {
            Dalamud.Conditions.ConditionChange -= ConditionChange;
        }
    }
}
