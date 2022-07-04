using System;
using System.Text.Json;
using Dalamud.Game.ClientState.Conditions;

namespace Ffxiv2Mqtt.Topic.Events
{
    internal sealed class ConditionsTopic : Topic, IDisposable
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

        public void Dispose()
        {
            DalamudServices.Conditions.ConditionChange -= ConditionChange;
        }
    }
}
