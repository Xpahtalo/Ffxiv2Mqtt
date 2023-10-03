using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class PlayerGathererStats : Topic, IDisposable
{
    private uint gp;
    private bool forcePublish;

    protected override     string        TopicPath    => "Player/Gatherer/CurrentStats";
    protected override     bool          Retained     => false;


    public PlayerGathererStats()
    {
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
        Service.PlayerEvents.OnJobChange        += JobChanged;
    }

    // Publish a message if GP changes.
    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        var shouldPublish = false;

        if (localPlayer.MaxGp != 0)
            TestValue(localPlayer.CurrentGp, ref gp, ref shouldPublish);

        if (shouldPublish || forcePublish) {
            forcePublish = false;
            Publish(new
                    {
                        GP = gp,
                    });
        }
    }

    // Publish current GP if the player changes to a gatherer.
    private void JobChanged(Job previousJob, Job currentJob)
    {
        if (currentJob.IsGatherer()) forcePublish = true;
    }

    public void Dispose()
    {
        Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated;
        Service.PlayerEvents.OnJobChange        -= JobChanged;
    }
}
