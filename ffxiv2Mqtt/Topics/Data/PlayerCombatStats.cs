using System;
using System.Text.Json;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Ffxiv2Mqtt.Topics.Data
{
    internal unsafe class PlayerCombatStatsTopic : Topic, IDisposable
    {
        [PluginService] private PlayerEvents? PlayerEvents { get; set; }

        private uint hp;
        private uint mp;
        private byte shield;

        protected override string TopicPath => "Player/Combat/Casting";
        protected override bool   Retained  => false;


        public override void Initialize()
        {
            if (PlayerEvents is not null) PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
        }

        private void PlayerUpdated(PlayerCharacter localPlayer)
        {
            var shouldPublish      = false;
            var localPlayerShields = ((Character*)localPlayer.Address)->ShieldValue;

            TestValue(localPlayer.CurrentHp, ref hp,     out shouldPublish);
            TestValue(localPlayer.CurrentMp, ref mp,     out shouldPublish);
            TestValue(localPlayerShields,    ref shield, out shouldPublish);

            if (shouldPublish) {
                Publish(JsonSerializer.Serialize(new
                                                 {
                                                     HP        = hp,
                                                     MP        = mp,
                                                     ShieldPct = shield,
                                                 }));
            }
        }

        public void Dispose()
        {
            if (PlayerEvents is not null) PlayerEvents.LocalPlayerUpdated -= PlayerUpdated;
        }
    }
}
