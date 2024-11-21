using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Enums;

namespace Ffxiv2Mqtt.Services;

internal class PlayerEvents : IDisposable
{
    private Job previousJob;

    // A delegate type used with the OnJobChange event.
    public delegate void OnJobChangeDelegate(Job previousJob, Job currentJob);

    // A delegate type used with the LocalPlayerUpdated event.
    public delegate void LocalPlayerUpdatedDelegate(IPlayerCharacter localPlayer);

    // Event that gets fired when the player changes jobs.
    public event OnJobChangeDelegate? OnJobChange;

    // Event that gets fired on evey framework update when the ClientState.LocalPlayer is not null.
    public event LocalPlayerUpdatedDelegate? LocalPlayerUpdated;


    public PlayerEvents()
    {
        Service.Log.Debug("PlayerEvents.Initialize: Framework.Update += Update");
        Service.Framework.Update += Update;
    }

    private void Update(IFramework framework)
    {
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null) return;

        var currentJob = (Job)localPlayer.ClassJob.RowId;
        if (currentJob != previousJob) {
#if DEBUG
            Service.Log.Debug($"{GetType().Name}.Update: Job changed from \"{previousJob}\" to \"{currentJob}\". Firing OnJobChange event.");
#endif
            OnJobChange?.Invoke(previousJob, currentJob);
        }

        previousJob = currentJob;


        LocalPlayerUpdated?.Invoke(localPlayer);
    }

    public void Dispose() { Service.Framework.Update -= Update; }
}
