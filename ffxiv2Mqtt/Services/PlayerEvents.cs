using System;
using Dalamud.Logging;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Ffxiv2Mqtt.Enums;

namespace Ffxiv2Mqtt.Services
{
    internal class PlayerEvents : IDisposable
    {
        [PluginService]
        public static ClientState ClientState { get; private set; } = null!;

        [PluginService]
        public static Framework Framework { get; private set; } = null!;

        // A delegate type used with the OnJobChange event.
        public delegate void OnJobChangeDelegate(Job previousJob, Job currentJob);

        // A delegate type used with the LocalPlayerUpdated event.
        public delegate void LocalPlayerUpdatedDelegate(PlayerCharacter localPlayer);

        // Event that gets fired when the player changes jobs.
        public event OnJobChangeDelegate? OnJobChange;

        // Event that gets fired on evey framework update when the ClientState.LocalPlayer is not null.
        public event LocalPlayerUpdatedDelegate? LocalPlayerUpdated;

        private Job previousJob;


        public static void Initialize(DalamudPluginInterface pluginInterface) => pluginInterface.Create<PlayerEvents>();

        public PlayerEvents(DalamudPluginInterface pluginInterface)
        {
            PluginLog.Debug("PlayerEvents.Initialize: Framework.Update += Update");
            Framework.Update += Update;
        }


        private void Update(Framework framework)
        {
            var localPlayer = ClientState.LocalPlayer;
            if (localPlayer == null) return;

            LocalPlayerUpdated?.Invoke(localPlayer);

            var currentJob = (Job)localPlayer.ClassJob.Id;
            if (currentJob != previousJob) {
#if DEBUG
                PluginLog.Debug($"{this.GetType().Name}.Update: Job changed from \"{previousJob}\" to \"{currentJob}\". Firing OnJobChange event.");
#endif
                OnJobChange?.Invoke(previousJob, currentJob);
            }

            previousJob = currentJob;
        }

        public void Dispose()
        {
            Framework.Update -= Update;
        }
    }
}