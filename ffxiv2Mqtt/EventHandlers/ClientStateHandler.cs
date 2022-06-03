using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.ClientState;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;

namespace Ffxiv2Mqtt.EventHandlers
{
    internal class ClientStateHandler
    {
        public static void Initialize(DalamudPluginInterface pluginInterface) =>
            pluginInterface.Create<ClientStateHandler>();
        
        [PluginService]
        [RequiredVersion("1.0")]
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        
        [PluginService]
        [RequiredVersion("1.0")]
        public static ClientState ClientState { get; private set; } = null!;
        
        [PluginService]
        [RequiredVersion("1.0")]
        public static DataManager DataManager { get; private set; } = null!;

        private MqttManager mqttManager;
        
        public ClientStateHandler(DalamudPluginInterface pluginInterface, MqttManager mqttManager)
        {
            ClientStateHandler.Initialize(pluginInterface);
            this.mqttManager = mqttManager;

            ClientState.CfPop += CfPop;
            ClientState.Login += Login;
            ClientState.Logout += Logout;
            ClientState.TerritoryChanged += TerritoryChanged;
        }

        public void Dispose()
        {
            ClientState.CfPop -= CfPop;
            ClientState.Login -= Login;
            ClientState.Logout -= Logout;
            ClientState.TerritoryChanged -= TerritoryChanged;
        }

        
        private void CfPop(object? s, ContentFinderCondition e)
        {
            mqttManager.PublishMessage("ClientState/Queue", "Pop");
        }

        private void Login(object? s, System.EventArgs e)
        {
            mqttManager.PublishMessage("ClientState/Login", "Login");
            mqttManager.PublishRetainedMessage("ClientState/LoggedInCharacter", ClientState.LocalPlayer.Name.ToString());
        }

        private void Logout(object? s, System.EventArgs e)
        {
            mqttManager.PublishMessage("ClientState/Login", "Logout");
            mqttManager.PublishRetainedMessage("ClientState/LoggedInCharacter", string.Empty);
        }

        private void TerritoryChanged(object? s, ushort e)
        {
            var territoryName = DataManager.GameData.Excel.GetSheet<TerritoryType>().GetRow(e).PlaceName.Value.Name;
            mqttManager.PublishMessage("ClientState/TerritoryChanged", territoryName.ToString());
        }
    }
}
