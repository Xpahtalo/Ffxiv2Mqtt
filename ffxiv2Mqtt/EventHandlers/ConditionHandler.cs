using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;

namespace Ffxiv2Mqtt.EventHandlers
{
    internal class ConditionHandler
    {
        public static void Initialize(DalamudPluginInterface pluginInterface) =>
    pluginInterface.Create<ClientStateHandler>();

        #region Plugin Services
        [PluginService]
        [RequiredVersion("1.0")]
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static Dalamud.Game.ClientState.Conditions.Condition Condition { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static DataManager DataManager { get; private set; } = null!;
        #endregion

        private MqttManager mqttManager;

        public ConditionHandler(DalamudPluginInterface pluginInterface, MqttManager mqttManager)
        {
            ConditionHandler.Initialize(pluginInterface);
            this.mqttManager = mqttManager;

            Condition.ConditionChange += ConditionChange;
        }

        private void ConditionChange(ConditionFlag flag, bool value)
        {
            var topic = "Condition/" + flag.ToString();
            mqttManager.PublishMessage(topic, value.ToString());
        }

        public void Dispose()
        {
            Condition.ConditionChange -= ConditionChange;
        }
    }
}
