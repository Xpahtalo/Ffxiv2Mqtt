using System;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace Ffxiv2Mqtt.Topics.Events;

internal class WorldInfoTopic : Topic, IDisposable
{
    private uint previousWorldId;

    protected override     string        TopicPath    => "Event/WorldChanged";
    protected override     bool          Retained     => true;


    public WorldInfoTopic() { Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated; }

    // Publish a message whenever the player changes worlds.
    private void PlayerUpdated(IPlayerCharacter localPlayer)
    {
        var shouldPublish = false;

        var currentWorld = localPlayer.CurrentWorld;
        var homeWorld    = localPlayer.HomeWorld;

        TestValue(currentWorld.RowId, ref previousWorldId, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        World = currentWorld.Value.Name.ToString(),
                        WorldId    = currentWorld.RowId,
                        Datacenter = currentWorld.Value.DataCenter.Value.Name.ToString(),
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
