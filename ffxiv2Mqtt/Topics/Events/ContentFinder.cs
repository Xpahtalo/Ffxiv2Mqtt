using System;
using Lumina.Excel.GeneratedSheets;

namespace Ffxiv2Mqtt.Topics.Events;

internal sealed class ContentFinder : Topic, IDisposable
{
    protected override string TopicPath => "Event/ContentFinder";
    protected override bool   Retained  => false;

    // ReSharper disable once MemberCanBePrivate.Global


    public ContentFinder() { Service.ClientState.CfPop += CfPop; }

    private void CfPop(ContentFinderCondition e) { Publish("Pop"); }

    public void Dispose() { Service.ClientState.CfPop -= CfPop; }
}
