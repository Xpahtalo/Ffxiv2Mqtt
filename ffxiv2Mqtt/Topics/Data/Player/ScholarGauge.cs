using System;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Extensions;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class ScholarGauge : Topic, IDisposable, IConfigurable
{
    private byte           aetherflow;
    private DismissedFairy dismissedFairy;
    private byte           fairyGauge;
    private short          seraphTimer;
    private short          syncTimer;

    protected override string TopicPath => "Player/JobGauge/SCH";
    protected override bool   Retained  => false;


    [PluginService] public Configuration? Configuration { get; set; }

    public ScholarGauge()
    {
        Configure();
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Configure()
    {
        if (Configuration is not null) syncTimer = (short)Configuration.Interval;
    }

    private void PlayerUpdated(IPlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if (!localPlayer.IsJob(Job.Scholar))
            return;
        var gauge = Service.JobGauges.Get<SCHGauge>();

        var shouldPublish = false;
        TestValue(gauge.Aetherflow,     ref aetherflow,     ref shouldPublish);
        TestValue(gauge.DismissedFairy, ref dismissedFairy, ref shouldPublish);
        TestValue(gauge.FairyGauge,     ref fairyGauge,     ref shouldPublish);
        TestCountDown(gauge.SeraphTimer, ref seraphTimer, syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Aetherflow,
                        gauge.DismissedFairy,
                        gauge.FairyGauge,
                        gauge.SeraphTimer,
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
