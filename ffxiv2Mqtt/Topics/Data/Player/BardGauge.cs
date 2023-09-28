using System;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class BardGauge : Topic, IDisposable, IConfigurable
{
    private readonly Song[] coda = new Song[3];
    private          Song   lastSong;
    private          byte   repertoire;
    private          Song   song;
    private          ushort songTimer;
    private          byte   soulVoice;
    private          ushort syncTimer;

    protected override string TopicPath => "Player/JobGauge/BRD";
    protected override bool   Retained  => false;


    [PluginService] public IJobGauges?    JobGauges     { get; set; }
    [PluginService] public IClientState?  ClientState   { get; set; }
    [PluginService] public Configuration? Configuration { get; set; }

    public BardGauge()
    {
        Configure();
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Configure()
    {
        if (Configuration is not null) syncTimer = (ushort)Configuration.Interval;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Bard)
            return;
        var gauge = Service.JobGauges.Get<BRDGauge>();

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

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
