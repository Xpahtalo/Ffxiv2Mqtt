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
    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        var shouldPublish = false;

        var currentWorld = localPlayer.CurrentWorld;
        var homeWorld    = localPlayer.HomeWorld;

        TestValue(currentWorld.Id, ref previousWorldId, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        World      = currentWorld?.GameData?.Name.ToString(),
                        WorldId    = currentWorld?.Id,
                        Datacenter = currentWorld?.GameData?.DataCenter?.Value?.Name.ToString(),
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
