using System;
using System.Text.Json;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Ffxiv2Mqtt.Topics.Events;

internal sealed class ReadyCheck : Topic, IDisposable
{
    private Hook<AgentReadyCheck.Delegates.EndReadyCheck>? ReadyCheckEndedHook;

    private Hook<AgentReadyCheck.Delegates.InitiateReadyCheck>? ReadyCheckStartedHook;

    public ReadyCheck()
    {
        var hookSuccess = HookReadyCheck();
        if (!hookSuccess) Service.Log.Warning("ReadyCheck topic will not function.");
    }

    protected override string TopicPath => "Event/ReadyCheck";
    protected override bool Retained => false;

    public void Dispose()
    {
        ReadyCheckStartedHook?.Dispose();
        ReadyCheckEndedHook?.Dispose();
    }

    private unsafe bool HookReadyCheck()
    {
        try
        {
            ReadyCheckStartedHook =
                Service.InteropProvider.HookFromAddress<AgentReadyCheck.Delegates.InitiateReadyCheck>(
                    AgentReadyCheck.MemberFunctionPointers.InitiateReadyCheck, ReadyCheckInitiatedDetour);
            ReadyCheckStartedHook.Enable();

            ReadyCheckEndedHook = Service.InteropProvider.HookFromAddress<AgentReadyCheck.Delegates.EndReadyCheck>(
                AgentReadyCheck.MemberFunctionPointers.EndReadyCheck, ReadyCheckEndedDetour);
            ReadyCheckEndedHook.Enable();
        }
        catch (Exception ex)
        {
            Service.Log.Error($"Failed to hook ready check functions.\n{ex}");
            return false;
        }

        return true;
    }

    private unsafe void ReadyCheckInitiatedDetour(AgentReadyCheck* agentReadyCheck)
    {
        ReadyCheckStartedHook!.Original(agentReadyCheck);
        Publish(JsonSerializer.Serialize(new
        {
            State = "Started"
        }));
    }

    private unsafe void ReadyCheckEndedDetour(AgentReadyCheck* agentReadyCheck)
    {
        ReadyCheckEndedHook!.Original(agentReadyCheck);

        var allReady = true;
        var allPresent = true;
        foreach (var entry in AgentReadyCheck.Instance()->ReadyCheckEntries)
        {
            if (entry.ContentId == 0)
                continue;
            switch (entry.Status)
            {
                case ReadyCheckStatus.NotReady:
                    allReady = false;
                    break;
                case ReadyCheckStatus.MemberNotPresent:
                    allPresent = false;
                    break;
            }
        }

        Publish(JsonSerializer.Serialize(
            new
            {
                State = "Finished",
                Ready = allReady,
                AllMembersPresent = allPresent
            }));
    }
}