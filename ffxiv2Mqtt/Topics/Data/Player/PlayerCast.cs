using System;
using System.Text.Json;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class PlayerCast : Topic, IDisposable
{
    private bool isCasting;

    protected override string TopicPath => "Player/Casting";
    protected override bool   Retained  => false;

    public PlayerCast() { Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated; }

    // Publish a message if the player either starts or stops casting.
    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        var shouldPublish = false;

        TestValue(localPlayer.IsCasting, ref isCasting, ref shouldPublish);

        if (shouldPublish)
            Publish(JsonSerializer.Serialize(new
                                             {
                                                 IsCasting = isCasting,
                                                 CastId    = localPlayer.CastActionId,
                                                 CastTime  = localPlayer.TotalCastTime,
                                             }));
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
