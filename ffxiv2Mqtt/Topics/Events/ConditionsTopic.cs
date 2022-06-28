using Newtonsoft.Json;
using Dalamud.Game.ClientState.Conditions;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Events
{
    internal class ConditionsTopic : Topic, ICleanable
    {

        
        internal ConditionsTopic(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Conditions";

            Dalamud.Conditions.ConditionChange += ConditionChange;
        }

        private void ConditionChange(ConditionFlag flag, bool value)
        {
            mqttManager.PublishMessage($"{topic}/{flag}", JsonConvert.SerializeObject(new { Id = (int)flag, Active = value }));
        }

        public void Cleanup()
        {
            Dalamud.Conditions.ConditionChange -= ConditionChange;
        }
    }
}
