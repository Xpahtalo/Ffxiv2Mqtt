using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace Ffxiv2Mqtt.Topics.Data;

internal class Party : Topic, IDisposable
{
    private int partyCount;

    protected override string TopicPath => "Party";
    protected override bool   Retained  => false;


    public Party()
    {
        Service.Framework.Update += FrameworkUpdate;
    }

    private unsafe void FrameworkUpdate(IFramework framework)
    {
        if (Service.DutyState.IsDutyStarted)
            return;

        if (InfoProxyCrossRealm.IsCrossRealmParty())
        {
            var partyProxy = InfoProxyCrossRealm.Instance();
            if (partyProxy is null)
                return;
            
            var count = 0;

            foreach (var group in partyProxy->CrossRealmGroups)
            foreach (var member in group.GroupMembers)
                if (member.ContentId != 0)
                    count++;

            if (count == partyCount)
                return;

            partyCount = count;
        }
        else
        {
            if (Service.PartyList.Count == partyCount)
                return;
            partyCount = Service.PartyList.Count;
        }

        Publish(new
        {
            Count = partyCount,
            CrossRealm = InfoProxyCrossRealm.IsCrossRealmParty()
        });
    }

    public void Dispose()
    {
        Service.Framework.Update -= FrameworkUpdate;
    }
}