using System;
using System.Text.Json;
using Dalamud.IoC;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data
{
    internal class PlayerCast : Topic, IDisposable
    {

        [PluginService] private PlayerEvents? PlayerEvents { get; set; }

        private bool isCasting;

        protected override string TopicPath => "Player/Casting";
        protected override bool   Retained  => false;


        public override void Initialize()
        {
            if (PlayerEvents is not null) PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
        }


        private void PlayerUpdated(PlayerCharacter localPlayer)
        {
            var shouldPublish = false;

            TestValue(localPlayer.IsCasting, ref isCasting, out shouldPublish);

            if (shouldPublish) {
                Publish(JsonSerializer.Serialize(new
                                                 {
                                                     IsCasting = isCasting,
                                                     CastId    = localPlayer.CastActionId,
                                                     CastTime  = localPlayer.TotalCastTime
                                                 }));
            }
        }

        public void Dispose()
        {
            if (PlayerEvents is not null) PlayerEvents.LocalPlayerUpdated -= PlayerUpdated;
        }
    }
}
