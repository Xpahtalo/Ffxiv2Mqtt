using System;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
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
    [PluginService] public IJobGauges?    JobGauges    { get; set; }
    [PluginService] public IClientState?  ClientState  { get; set; }

    public override void Initialize()
    {
        PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (ClientState!.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Astrologian)
            return;
        var gauge = JobGauges?.Get<ASTGauge>();
        if (gauge is null)
            return;

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


    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
