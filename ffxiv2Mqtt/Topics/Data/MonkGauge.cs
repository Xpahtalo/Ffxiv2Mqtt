using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data;

internal class MonkGauge : Topic, IDisposable, IConfigurable
{
    private          byte          chakra;
    private readonly BeastChakra[] beastChakra;
    private          ushort        blitzTimeRemaining;
    private          int           syncTimer;

    protected override string TopicPath => "Player/JobGauge/MNK";
    protected override bool   Retained  => false;

    [PluginService] public PlayerEvents?  PlayerEvents  { get; set; }
    [PluginService] public JobGauges?     JobGauges     { get; set; }
    [PluginService] public ClientState?   ClientState   { get; set; }
    [PluginService] public Configuration? Configuration { get; set; }


    public MonkGauge()
    {
        beastChakra = new BeastChakra[3];
    }

    public override void Initialize()
    {
        Configure();
        PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Configure()
    {
        if (Configuration is not null) syncTimer = Configuration.Interval;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (ClientState!.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Monk)
            return;
        var gauge = JobGauges?.Get<MNKGauge>();
        if (gauge is null)
            return;

        var shouldPublish = false;
        TestValue(gauge.Chakra, ref chakra, ref shouldPublish);
        for (var i = 0; i < 3; i++) TestValue(gauge.BeastChakra[i], ref beastChakra[i], ref shouldPublish);
        TestCountDown(gauge.BlitzTimeRemaining, ref blitzTimeRemaining, syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Chakra,
                        gauge.BeastChakra,
                        gauge.BlitzTimeRemaining,
                    });
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
