using System;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Events
{
    internal sealed class TerritoryTopic : Topic,ICleanable, IDisposable
    {
        public string Name
        {
            get
            {
                var resolvedTerritory = DalamudServices.GameData?.Excel?.GetSheet<Lumina.Excel.GeneratedSheets.TerritoryType>()?.GetRow(territory)?.PlaceName?.Value?.Name;
                if (resolvedTerritory is not null)
                    return resolvedTerritory.ToString();
                else
                    return "Unknown";
            }
        }
        public ushort ID { get => territory; }

        ushort territory;

        public TerritoryTopic(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Event/TerritoryChanged";
            DalamudServices.ClientState.TerritoryChanged += TerritoryChanged;
        }

        internal void TerritoryChanged(object? s, ushort e)
        {
            territory = e;
            Publish(true);
        }

        public void Cleanup()
        {
            mqttManager.PublishMessage(topic, "");
        }

        public void Dispose()
        {
            DalamudServices.ClientState.TerritoryChanged -= TerritoryChanged;
        }
    }
}