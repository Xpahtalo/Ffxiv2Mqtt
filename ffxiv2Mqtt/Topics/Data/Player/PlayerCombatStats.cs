using System;
using System.Text.Json;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal unsafe class PlayerCombatStats : Topic, IDisposable
{
    private uint hp;
    private uint mp;
    private byte shield;

    protected override string TopicPath => "Player/Combat/Stats";
    protected override bool   Retained  => false;

    public PlayerCombatStats() { Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated; }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        var shouldPublish = false;

        var localPlayerShields = ((Character*)localPlayer.Address)->CharacterData.ShieldValue;

        TestValue(localPlayer.CurrentHp, ref hp,     ref shouldPublish);
        TestValue(localPlayer.CurrentMp, ref mp,     ref shouldPublish);
        TestValue(localPlayerShields,    ref shield, ref shouldPublish);

        if (shouldPublish)
            Publish(JsonSerializer.Serialize(new
                                             {
                                                 HP        = hp,
                                                 MP        = mp,
                                                 ShieldPct = shield,
                                             }));
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
