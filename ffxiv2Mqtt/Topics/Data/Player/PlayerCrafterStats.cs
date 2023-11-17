using System;
using System.Text.Json;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class PlayerCrafterStats : Topic, IDisposable
{
    private uint cp;

    protected override     string        TopicPath    => "Player/Crafter/CurrentStats";
    protected override     bool          Retained     => false;

    public PlayerCrafterStats() { Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated; }

    // Send a message when the player's CP changes.
    private void PlayerUpdated(IPlayerCharacter localPlayer)
    {
        var shouldUpdate = false;

        TestValue(localPlayer.CurrentCp, ref cp, ref shouldUpdate);

        if (shouldUpdate)
            Publish(JsonSerializer.Serialize(new
                                             {
                                                 CP = cp,
                                             }));
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
