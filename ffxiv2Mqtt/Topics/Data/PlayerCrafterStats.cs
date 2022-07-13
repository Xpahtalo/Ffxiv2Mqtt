using System;
using System.Text.Json;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data;

internal class PlayerCrafterStats : Topic, IDisposable
{
    private uint cp;

    protected override     string        TopicPath    => "Player/Crafter/CurrentStats";
    protected override     bool          Retained     => false;
    [PluginService] public PlayerEvents? PlayerEvents { get; set; }

    public override void Initialize()
    {
        PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
    }

    // Send a message when the player's CP changes.
    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        var shouldUpdate = false;

        TestValue(localPlayer.CurrentCp, ref cp, ref shouldUpdate);

        if (shouldUpdate)
            Publish(JsonSerializer.Serialize(new
                                             {
                                                 CP = cp,
                                             }));
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
