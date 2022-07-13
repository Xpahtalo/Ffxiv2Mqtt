using System;
using System.Text.Json;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Ffxiv2Mqtt.Topics.Data;

internal unsafe class PlayerCombatStats : Topic, IDisposable
{
    private uint hp;
    private uint mp;
    private byte shield;

    protected override string TopicPath => "Player/Combat/Stats";
    protected override bool   Retained  => false;

    // ReSharper disable once MemberCanBePrivate.Global
    [PluginService] public PlayerEvents? PlayerEvents { get; set; }


    public override void Initialize()
    {
        if (PlayerEvents is not null) PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        var shouldPublish      = false;
        var localPlayerShields = ((Character*)localPlayer.Address)->ShieldValue;

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

    public void Dispose()
    {
        if (PlayerEvents is not null) PlayerEvents.LocalPlayerUpdated -= PlayerUpdated;
    }
}
