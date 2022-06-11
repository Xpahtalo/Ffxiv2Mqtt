using System.Threading;
using System.Threading.Tasks;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Logging;
using Dalamud.Game.ClientState;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;

namespace Ffxiv2Mqtt.EventHandlers
{
    internal class ClientStateHandler
    {
        public static void Initialize(DalamudPluginInterface pluginInterface) =>
            pluginInterface.Create<ClientStateHandler>();

        #region Plugin Services
        [PluginService]
        [RequiredVersion("1.0")]
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        
        [PluginService]
        [RequiredVersion("1.0")]
        public static ClientState ClientState { get; private set; } = null!;
        
        [PluginService]
        [RequiredVersion("1.0")]
        public static DataManager DataManager { get; private set; } = null!;
        #endregion
        
        private MqttManager mqttManager;
        
        public ClientStateHandler(DalamudPluginInterface pluginInterface, MqttManager mqttManager)
        {
            ClientStateHandler.Initialize(pluginInterface);
            this.mqttManager = mqttManager;

            ClientState.CfPop += CfPop;
            ClientState.Login += Login;
            ClientState.Logout += Logout;
            ClientState.TerritoryChanged += TerritoryChanged;

            PluginLog.Information("ClientStateHandler initialized");
        }

        private void CfPop(object? s, ContentFinderCondition e)
        {
            mqttManager.PublishMessage("ClientState/Queue", "Pop");
        }

        private void Login(object? s, System.EventArgs e)
        {
            mqttManager.PublishMessage("ClientState/Login", "LoggedIn", true);
            Task.Run(() =>
            {
                while (ClientState?.LocalPlayer?.Name == null)
                    Thread.Sleep(1000);
                mqttManager.PublishMessage("ClientState/LoggedInCharacter", ClientState.LocalPlayer.Name.ToString(), true);
            });
        }

        private void Logout(object? s, System.EventArgs e)
        {
            mqttManager.PublishMessage("ClientState/Login", "LoggedOut", true);
            mqttManager.PublishMessage("ClientState/LoggedInCharacter", string.Empty, true);
        }

        private void TerritoryChanged(object? s, ushort e)
        {
            var territoryName = DataManager?.GameData?.Excel?.GetSheet<TerritoryType>()?.GetRow(e)?.PlaceName?.Value?.Name;
            if (territoryName != null)
                mqttManager.PublishMessage("ClientState/TerritoryChanged", territoryName.ToString(), true);
        }
        
        public void Dispose()
        {
            mqttManager.PublishMessage("ClientState/Territory", "", true);
            mqttManager.PublishMessage("ClientState/LoggedInCharacter", "", true);

            ClientState.CfPop -= CfPop;
            ClientState.Login -= Login;
            ClientState.Logout -= Logout;
            ClientState.TerritoryChanged -= TerritoryChanged;
        }
    }
}
