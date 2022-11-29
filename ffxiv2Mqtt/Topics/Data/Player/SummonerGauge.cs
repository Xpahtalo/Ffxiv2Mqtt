using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class SummonerGauge : Topic, IDisposable, IConfigurable
{
    private byte   aetherflowStacks;
    private byte   attunement;
    private ushort attunmentTimerRemaining;
    private bool   isBahamutReady;
    private bool   isPhoenixReady;
    private bool   isGarudaReady;
    private bool   isGarudaAttuned;
    private bool   isIfritReady;
    private bool   isIfritAttuned;
    private bool   isTitanReady;
    private bool   isTitanAttuned;
    private ushort summonTimeRemaining;
    private int    syncTimer;

    protected override string TopicPath => "Player/JobGauge/SMN";
    protected override bool   Retained  => false;

    [PluginService] public PlayerEvents?  PlayerEvents  { get; set; }
    [PluginService] public JobGauges?     JobGauges     { get; set; }
    [PluginService] public ClientState?   ClientState   { get; set; }
    [PluginService] public Configuration? Configuration { get; set; }

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
        if ((Job)localPlayer.ClassJob.Id != Job.Summoner)
            return;
        var gauge = JobGauges?.Get<SMNGauge>();
        if (gauge is null)
            return;

        var shouldPublish = false;
        TestValue(gauge.AetherflowStacks, ref aetherflowStacks, ref shouldPublish);
        TestValue(gauge.Attunement,       ref attunement,       ref shouldPublish);
        TestValue(gauge.IsBahamutReady,   ref isBahamutReady,   ref shouldPublish);
        TestValue(gauge.IsPhoenixReady,   ref isPhoenixReady,   ref shouldPublish);
        TestValue(gauge.IsGarudaReady,    ref isGarudaReady,    ref shouldPublish);
        TestValue(gauge.IsGarudaAttuned,  ref isGarudaAttuned,  ref shouldPublish);
        TestValue(gauge.IsIfritReady,     ref isIfritReady,     ref shouldPublish);
        TestValue(gauge.IsIfritAttuned,   ref isIfritAttuned,   ref shouldPublish);
        TestValue(gauge.IsTitanReady,     ref isTitanReady,     ref shouldPublish);
        TestValue(gauge.IsTitanAttuned,   ref isTitanAttuned,   ref shouldPublish);
        TestCountDown(gauge.AttunmentTimerRemaining, ref attunmentTimerRemaining, syncTimer, ref shouldPublish);
        TestCountDown(gauge.SummonTimerRemaining,    ref summonTimeRemaining,     syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.AetherflowStacks,
                        gauge.Attunement,
                        gauge.AttunmentTimerRemaining,
                        gauge.IsBahamutReady,
                        gauge.IsPhoenixReady,
                        gauge.IsGarudaReady,
                        gauge.IsIfritReady,
                        gauge.IsTitanReady,
                        gauge.IsGarudaAttuned,
                        gauge.IsIfritAttuned,
                        gauge.IsTitanAttuned,
                        gauge.SummonTimerRemaining,
                    });
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
