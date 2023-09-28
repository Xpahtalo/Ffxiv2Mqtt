using System;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class AstrologianGauge : Topic, IDisposable
{
    private          CardType   drawnCard;
    private          CardType   drawnCrownType;
    private readonly SealType[] seals = new SealType[3];

    protected override     string        TopicPath    => "Player/JobGauge/AST";
    protected override     bool          Retained     => false;
    [PluginService] public PlayerEvents? PlayerEvents { get; set; }

    public AstrologianGauge() { Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated; }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Astrologian)
            return;
        var gauge = Service.JobGauges.Get<ASTGauge>();

        var shouldPublish = false;
        TestValue(gauge.DrawnCard,      ref drawnCard,      ref shouldPublish);
        TestValue(gauge.DrawnCrownCard, ref drawnCrownType, ref shouldPublish);
        for (var i = 0; i < seals.Length; i++) TestValue(gauge.Seals[i], ref seals[i], ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.DrawnCard,
                        gauge.DrawnCrownCard,
                        gauge.Seals,
                    });
    }


    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
