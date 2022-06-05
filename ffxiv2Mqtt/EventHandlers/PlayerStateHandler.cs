using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;

namespace Ffxiv2Mqtt.EventHandlers
{
    internal class PlayerStateHandler
    {
        public static void Initialize(DalamudPluginInterface pluginInterface) =>
    pluginInterface.Create<PlayerStateHandler>();

        #region PluginServices

        [PluginService]
        [RequiredVersion("1.0")]
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ClientState ClientState { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static Framework Framework { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static DataManager DataManager { get; private set; } = null!;

        #endregion

        private MqttManager mqttManager;

        private uint currentCp;
        private uint currentGp;
        private uint currentHp;
        private uint currentMp;
        private byte level;


        public PlayerStateHandler(DalamudPluginInterface pluginInterface, MqttManager mqttManager)
        {
            PlayerStateHandler.Initialize(pluginInterface);
            this.mqttManager = mqttManager;

            Framework.Update += Update;
        }

        private void Update(Framework framework)
        {
            var localPlayer = ClientState.LocalPlayer;
            if (localPlayer == null)
                return;


            mqttManager.TestValue(localPlayer.CurrentCp, ref currentCp, "Player/CurrentCP");
            if (localPlayer.MaxGp != 0)
                mqttManager.TestValue(localPlayer.CurrentGp, ref currentGp, "Player/CurrentGP");
            mqttManager.TestValue(localPlayer.CurrentHp, ref currentHp, "Player/CurrentHP");
            mqttManager.TestValue(localPlayer.CurrentMp, ref currentMp, "Player/CurrentMP");
            mqttManager.TestValue(localPlayer.Level, ref level, "Player/Level", true);
        }

        public void Dispose()
        {
            mqttManager.PublishRetainedMessage("Player/Level", "");

            Framework.Update -= Update;
        }
    }
}
