using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data;

internal class WorldInfoTopic : Topic, IDisposable
{
    private uint previousWorldId;

    protected override     string        TopicPath    => "Event/WorldChanged";
    protected override     bool          Retained     => true;
    [PluginService] public PlayerEvents? PlayerEvents { get; set; }


    public override void Initialize()
    {
        PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
    }

    // Publish a message whenever the player changes worlds.
    public void PlayerUpdated(PlayerCharacter localPlayer)
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

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
