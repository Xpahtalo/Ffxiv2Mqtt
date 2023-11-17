using System;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Ffxiv2Mqtt.Enums;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class AstrologianGauge : Topic, IDisposable {
    private readonly CardType[] drawnCards;
    private          CardType   drawnCrownCard;
    private          DrawType   activeDraw;

    protected override string TopicPath => "Player/JobGauge/AST";
    protected override bool   Retained  => false;


    public AstrologianGauge() {
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
        drawnCards                              =  new CardType[3];
    }

    private void PlayerUpdated(IPlayerCharacter localPlayer) {
        if (Service.ClientState.IsPvP) {
            return;
        }

        if ((Job)localPlayer.ClassJob.Id != Job.Astrologian) {
            return;
        }

        var gauge = Service.JobGauges.Get<ASTGauge>();

        var shouldPublish = false;
        for (var i = 0; i < drawnCards.Length; i++) {
            TestValue(gauge.DrawnCards[i], ref drawnCards[i], ref shouldPublish);
        }

        TestValue(gauge.DrawnCrownCard, ref drawnCrownCard, ref shouldPublish);
        TestValue(gauge.ActiveDraw,     ref activeDraw,     ref shouldPublish);

        if (shouldPublish) {
            Publish(new {
                gauge.DrawnCards,
                gauge.DrawnCrownCard,
                gauge.ActiveDraw,
            });
        }
    }


    public void Dispose() {
        Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated;
    }
}