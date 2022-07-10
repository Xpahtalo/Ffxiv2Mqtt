using System;
using System.Text.Json;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data
{
    internal class PlayerGathererStatsTopic : Topic, IDisposable
    {
        [PluginService] public PlayerEvents? PlayerEvents { get; set; }

        protected override string TopicPath => "Player/Gatherer/CurrentStats";
        protected override bool   Retained  => false;

        private uint gp;
        private bool forcePublish;

        public override void Initialize()
        {
            PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
            PlayerEvents!.OnJobChange        += JobChanged;
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
            if (currentJob.IsGatherer()) {
                forcePublish = true;
            }
        }

        public void Dispose()
        {
            PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
            PlayerEvents!.OnJobChange        -= JobChanged;
        }
    }
}
