using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data;

internal class MachinistGauge : Topic, IDisposable, IConfigurable
{
    private byte  battery;
    private byte  heat;
    private bool  isOverheated;
    private short overheatTimeRemaining;
    private bool  isRobotActive;
    private short summonTimeRemaining;
    private byte  lastSummonBatteryPower;
    private int   syncTimer;

    protected override string TopicPath => "Player/JobGauge/MCH";
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
        if ((Job)localPlayer.ClassJob.Id != Job.Machinist)
            return;
        var gauge = JobGauges?.Get<MCHGauge>();
        if (gauge is null)
            return;

        var shouldPublish = false;
        TestValue(gauge.Battery,                ref battery,                ref shouldPublish);
        TestValue(gauge.Heat,                   ref heat,                   ref shouldPublish);
        TestValue(gauge.IsOverheated,           ref isOverheated,           ref shouldPublish);
        TestValue(gauge.IsRobotActive,          ref isRobotActive,          ref shouldPublish);
        TestValue(gauge.LastSummonBatteryPower, ref lastSummonBatteryPower, ref shouldPublish);
        TestCountDown(gauge.OverheatTimeRemaining, ref overheatTimeRemaining, syncTimer, ref shouldPublish);
        TestCountDown(gauge.SummonTimeRemaining,   ref summonTimeRemaining,   syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Battery,
                        gauge.Heat,
                        OverheatActive = gauge.IsOverheated,
                        gauge.OverheatTimeRemaining,
                        RobotActive        = gauge.IsRobotActive,
                        RobotTimeRemaining = gauge.SummonTimeRemaining,
                        gauge.LastSummonBatteryPower,
                    });
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
