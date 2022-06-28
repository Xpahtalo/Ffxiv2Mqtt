using System.Text.Json;
using Dalamud.Game.ClientState.Conditions;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Events
{
    internal class ConditionsTopic : Topic, ICleanable
    {

        
        internal ConditionsTopic(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Conditions";

            DalamudServices.Conditions.ConditionChange += ConditionChange;
        }

        private void ConditionChange(ConditionFlag flag, bool value)
        {
            var o = new
            {
                Id = (int)flag,
                Active = value
            };
            mqttManager.PublishMessage($"{topic}/{flag}", JsonSerializer.Serialize(o));
        }

        public void Cleanup()
        {
            DalamudServices.Conditions.ConditionChange -= ConditionChange;
        }
    }
}
