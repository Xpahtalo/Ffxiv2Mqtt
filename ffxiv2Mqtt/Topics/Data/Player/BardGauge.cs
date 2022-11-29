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

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class BardGauge : Topic, IDisposable, IConfigurable
{
    private readonly Song[] coda;
    private          Song   lastSong;
    private          byte   repertoire;
    private          Song   song;
    private          ushort songTimer;
    private          byte   soulVoice;
    private          int    syncTimer;

    protected override string TopicPath => "Player/JobGauge/BRD";
    protected override bool   Retained  => false;

    [PluginService] public PlayerEvents?  PlayerEvents  { get; set; }
    [PluginService] public JobGauges?     JobGauges     { get; set; }
    [PluginService] public ClientState?   ClientState   { get; set; }
    [PluginService] public Configuration? Configuration { get; set; }

    public BardGauge()
    {
        coda = new Song[3];
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
        if ((Job)localPlayer.ClassJob.Id != Job.Bard)
            return;
        var gauge = JobGauges?.Get<BRDGauge>();
        if (gauge is null)
            return;

        var shouldPublish = false;
        for (var i = 0; i < coda.Length; i++) TestValue(gauge.Coda[i], ref coda[i], ref shouldPublish);

        TestValue(gauge.LastSong,   ref lastSong,   ref shouldPublish);
        TestValue(gauge.Repertoire, ref repertoire, ref shouldPublish);
        TestValue(gauge.Song,       ref song,       ref shouldPublish);
        TestValue(gauge.SoulVoice,  ref soulVoice,  ref shouldPublish);
        TestCountDown(gauge.SongTimer, ref songTimer, syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Song,
                        gauge.SongTimer,
                        gauge.SoulVoice,
                        gauge.Coda,
                        gauge.Repertoire,
                        gauge.LastSong,
                    });
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
