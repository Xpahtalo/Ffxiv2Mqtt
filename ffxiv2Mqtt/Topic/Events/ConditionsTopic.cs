using Newtonsoft.Json;
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
            mqttManager.PublishMessage($"{topic}/{flag}", JsonConvert.SerializeObject(new { Id = (int)flag, Active = value }));
        }

        public void Cleanup()
        {
            DalamudServices.Conditions.ConditionChange -= ConditionChange;
        }
    }
}
