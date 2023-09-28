﻿using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class BlackMageGauge : Topic, IDisposable, IConfigurable
{
    private bool  isEnochianActive;
    private bool  isParadoxActive;
    private byte  astralFireStacks;
    private byte  umbralIceStacks;
    private short elementTimeRemaining;
    private short enochianTimeRemaining;
    private byte  umbralHearts;
    private byte  polyglotStacks;
    private short syncTimer;

    protected override string TopicPath => "Player/JobGauge/BLM";
    protected override bool   Retained  => false;

    [PluginService] public IJobGauges?    JobGauges     { get; set; }
    [PluginService] public IClientState?  ClientState   { get; set; }
    [PluginService] public Configuration? Configuration { get; set; }

    public BlackMageGauge()
    {
        Configure();
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Configure()
    {
        if (Configuration is not null) syncTimer = (short)Configuration.Interval;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if (!((Job)localPlayer.ClassJob.Id == Job.BlackMage || (Job)localPlayer.ClassJob.Id == Job.Thaumaturge))
            return;
        var gauge = Service.JobGauges.Get<BLMGauge>();

        var shouldPublish = false;
        TestValue(gauge.IsEnochianActive, ref isEnochianActive, ref shouldPublish);
        TestValue(gauge.IsParadoxActive,  ref isParadoxActive,  ref shouldPublish);
        TestValue(gauge.AstralFireStacks, ref astralFireStacks, ref shouldPublish);
        TestValue(gauge.UmbralIceStacks,  ref umbralIceStacks,  ref shouldPublish);
        TestValue(gauge.UmbralHearts,     ref umbralHearts,     ref shouldPublish);
        TestValue(gauge.PolyglotStacks,   ref polyglotStacks,   ref shouldPublish);
        TestCountDown(gauge.ElementTimeRemaining, ref elementTimeRemaining,  syncTimer, ref shouldPublish);
        TestCountDown(gauge.EnochianTimer,        ref enochianTimeRemaining, syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        EnochianActive        = gauge.IsEnochianActive,
                        EnochianTimeRemaining = gauge.EnochianTimer,
                        ParadoxActive         = gauge.IsParadoxActive,
                        gauge.AstralFireStacks,
                        gauge.UmbralIceStacks,
                        ElementTimeRemaining = gauge.EnochianTimer,
                        gauge.UmbralHearts,
                        gauge.PolyglotStacks,
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
