using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Extensions;
using CanvasFlags = Dalamud.Game.ClientState.JobGauge.Enums.CanvasFlags;
using CreatureFlags = Dalamud.Game.ClientState.JobGauge.Enums.CreatureFlags;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class PictomancerGauge : Topic, IDisposable {
    private byte          palleteGauge;
    private byte          paint;
    private CreatureFlags creatureFlags;
    private CanvasFlags   canvasFlags;


    protected override string TopicPath => "Player/JobGauge/PCT";
    protected override bool   Retained  => false;

    public PictomancerGauge() {
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Dispose() {
        Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated;
    }

    private void PlayerUpdated(IPlayerCharacter localPlayer) {
        if (Service.ClientState.IsPvP) {
            return;
        }

        if (!localPlayer.IsJob(Job.Pictomancer)) {
            return;
        }

        var gauge         = Service.JobGauges.Get<PCTGauge>();
        var shouldPublish = false;

        TestValue(gauge.PalleteGauge,  ref palleteGauge,  ref shouldPublish);
        TestValue(gauge.Paint,         ref paint,         ref shouldPublish);
        TestValue(gauge.CreatureFlags, ref creatureFlags, ref shouldPublish);
        TestValue(gauge.CanvasFlags,   ref canvasFlags,   ref shouldPublish);

        if (shouldPublish) {
            var creatureMotif = gauge.CreatureFlags switch {
                CreatureFlags.Pom   => "Pom",
                CreatureFlags.Wings => "Wings",
                CreatureFlags.Claw  => "Claw",
                _                   => "none",
            };

            var muse = gauge.CreatureFlags switch {
                CreatureFlags.MooglePortait  => "Moogle",
                CreatureFlags.MadeenPortrait => "Madeen",
                _                            => "none",
            };

            var weaponMotif = gauge.WeaponMotifDrawn switch {
                true  => "Hammer",
                false => "none",
            };

            var landscapeMotif = gauge.WeaponMotifDrawn switch {
                true  => "StarrySky",
                false => "none",
            };

            Publish(new {
                SubtractivePallete = gauge.PalleteGauge,
                WhitePaint         = gauge.Paint,
                Muse               = muse,
                CreatureCanvas     = creatureMotif,
                WeaponCanvas       = weaponMotif,
                LandscapeCanvas    = landscapeMotif,
            });
        }
    }
}