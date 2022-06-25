
namespace Ffxiv2Mqtt.TopicTracker
{
    internal class TerritoryTracker : BaseTopicTracker
    {
        public string Name
        {
            get
            {
                var resolvedTerritory = Dalamud.GameData?.Excel?.GetSheet<Lumina.Excel.GeneratedSheets.TerritoryType>()?.GetRow(territory)?.PlaceName?.Value?.Name;
                if (resolvedTerritory is not null)
                    return resolvedTerritory.ToString();
                else
                    return "Unknown";
            }
        }

        ushort territory;

        public TerritoryTracker(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Territory";
        }

        internal void TerritoryChanged(object? s, ushort e)
        {
            territory = e;
            Publish();
        }

    }
}