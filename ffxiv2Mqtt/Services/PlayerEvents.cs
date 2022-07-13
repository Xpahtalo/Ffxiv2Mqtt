using System;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Ffxiv2Mqtt.Enums;

namespace Ffxiv2Mqtt.Services;

internal class PlayerEvents : IDisposable
{
    private Job previousJob;

    [PluginService]
    public static ClientState ClientState { get; } = null!;

    [PluginService]
    public static Framework Framework { get; } = null!;

    // A delegate type used with the OnJobChange event.
    public delegate void OnJobChangeDelegate(Job previousJob, Job currentJob);

    // A delegate type used with the LocalPlayerUpdated event.
    public delegate void LocalPlayerUpdatedDelegate(PlayerCharacter localPlayer);

    // Event that gets fired when the player changes jobs.
    public event OnJobChangeDelegate? OnJobChange;

    // Event that gets fired on evey framework update when the ClientState.LocalPlayer is not null.
    public event LocalPlayerUpdatedDelegate? LocalPlayerUpdated;


    public static void Initialize(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<PlayerEvents>();
    }

    public PlayerEvents(DalamudPluginInterface pluginInterface)
    {
        PluginLog.Debug("PlayerEvents.Initialize: Framework.Update += Update");
        Framework.Update += Update;
    }


    private void Update(Framework framework)
    {
        var localPlayer = ClientState.LocalPlayer;
        if (localPlayer == null) return;

        var currentJob = (Job)localPlayer.ClassJob.Id;
        if (currentJob != previousJob) {
#if DEBUG
            PluginLog.Debug($"{GetType().Name}.Update: Job changed from \"{previousJob}\" to \"{currentJob}\". Firing OnJobChange event.");
#endif
            OnJobChange?.Invoke(previousJob, currentJob);
        }

        previousJob = currentJob;


        LocalPlayerUpdated?.Invoke(localPlayer);
    }

    public void Dispose()
    {
        Framework.Update -= Update;
    }
}
