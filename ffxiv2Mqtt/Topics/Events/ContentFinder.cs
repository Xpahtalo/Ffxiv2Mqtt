using System;
using Dalamud.Game.ClientState;
using Dalamud.IoC;

namespace Ffxiv2Mqtt.Topics.Events
{
    internal sealed class ContentFinder : Topic, IDisposable
    {
        [PluginService]
        // ReSharper disable once MemberCanBePrivate.Global
        public ClientState? ClientState { get; set; }

        protected override string TopicPath => "Event/ContentFinder";
        protected override bool   Retained  => false;


        public override void Initialize()
        {
            if (ClientState is not null) ClientState.CfPop += CfPop;
        }

        private void CfPop(object? s, Lumina.Excel.GeneratedSheets.ContentFinderCondition e)
        {
            Publish("Pop");
        }

        public void Dispose()
        {
            if (ClientState is not null) ClientState.CfPop -= CfPop;
        }
    }
}
