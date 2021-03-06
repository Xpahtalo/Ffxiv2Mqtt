using System;
using System.Text.Json;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.IoC;
using Ffxiv2Mqtt.Topics.Interfaces;
using Lumina.Excel.GeneratedSheets;

namespace Ffxiv2Mqtt.Topics.Events;

internal sealed class Territory : Topic, ICleanable, IDisposable
{
    protected override     string       TopicPath   => "Event/TerritoryChanged";
    protected override     bool         Retained    => true;
    [PluginService] public ClientState? ClientState { get; set; }
    [PluginService] public DataManager? DataManager { get; set; }

    public override void Initialize()
    {
        ClientState!.TerritoryChanged += TerritoryChanged;
    }

    // Publish a message whenever the player changes territories.
    private void TerritoryChanged(object? o, ushort territoryId)
    {
        var territoryRow = DataManager?.Excel.GetSheet<TerritoryType>()?.GetRow(territoryId);
        if (territoryRow is null) return;
        Publish(JsonSerializer.Serialize(new
                                         {
                                             Name     = territoryRow.PlaceName?.Value?.Name.ToString() ?? "Unknown",
                                             ID       = territoryId,
                                             Region   = territoryRow.PlaceNameRegion?.Value?.Name.ToString(),
                                             RegionID = territoryRow.PlaceNameRegion?.Value?.RowId,
                                         }));
    }

    // Remove retained messages when exiting 
    public void Cleanup()
    {
        Publish("");
    }

    public void Dispose()
    {
        ClientState!.TerritoryChanged -= TerritoryChanged;
    }
}
