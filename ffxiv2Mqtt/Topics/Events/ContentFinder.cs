using System;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;

namespace Ffxiv2Mqtt.Topics.Events;

internal sealed class ContentFinder : Topic, IDisposable
{
    protected override string TopicPath => "Event/ContentFinder";
    protected override bool   Retained  => false;

    // ReSharper disable once MemberCanBePrivate.Global
    [PluginService] public IClientState? ClientState { get; set; }


    public override void Initialize()
    {
        if (ClientState is not null) ClientState.CfPop += CfPop;
    }

    private void CfPop(ContentFinderCondition e)
    {
        Publish("Pop");
    }

    public void Dispose()
    {
        if (ClientState is not null) ClientState.CfPop -= CfPop;
    }
}
