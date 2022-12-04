using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Party;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Ffxiv2Mqtt.Topics.Data;

internal unsafe class Party : Topic, IDisposable
{
    private          int                   partyCount;
    private readonly JsonSerializerOptions serializerOptions;
    private          bool                  dutyStarted;
    private          bool                  completedThisTerritory;

    private delegate byte DutyEventDelegate(void* a1, void* a2, ushort* a3);

    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B D9 49 8B F8 41 0F B7 08", DetourName = nameof(DutyEventFunction))]
    private readonly Hook<DutyEventDelegate>? dutyEventHook = null;

    protected override string TopicPath => "Party";
    protected override bool   Retained  => false;

    [PluginService] public PartyList?   PartyList   { get; set; }
    [PluginService] public Framework?   Framework   { get; set; }
    [PluginService] public Condition?   Condition   { get; set; }
    [PluginService] public ClientState? ClientState { get; set; }

    public Party()
    {
        serializerOptions = new JsonSerializerOptions
                            {
                                Converters = { new PartyMemberSerializer() },
                            };
    }

    public override void Initialize()
    {
        if (Framework is not null) Framework.Update               += FrameworkUpdate;
        if (ClientState is not null) ClientState.TerritoryChanged += TerritoryChanged;

        SignatureHelper.Initialise(this);

        dutyEventHook?.Enable();

        if (BoundByDuty()) {
            dutyStarted = true;
        }
    }

    private void FrameworkUpdate(Framework framework)
    {
        // Tracking combat status
        if (!dutyStarted && !completedThisTerritory) {
            if (BoundByDuty() && Condition![ConditionFlag.InCombat]) {
                dutyStarted = true;
            }
        }


        // The party list is empty when transitioning, so block all changes.
        if (Condition is not null && (Condition[ConditionFlag.BetweenAreas] || Condition[ConditionFlag.BetweenAreas51]))
            return;

        // Party list size changes as players leave after completing the duty. Actual changes to the party will be caught
        // after returning to the overworld.
        if (completedThisTerritory)
            return;

        // Assuming none of the previous blocking conditions are true, send party messages if the count has changed.
        if (PartyList!.Length == partyCount)
            return;

        partyCount = PartyList.Length;
        Publish($"{TopicPath}/Count",   partyCount.ToString());
        Publish($"{TopicPath}/Members", JsonSerializer.Serialize(PartyList, serializerOptions));
    }

    private byte DutyEventFunction(void* a1, void* a2, ushort* a3)
    {
        try {
            var category = *(a3);
            var type     = *(uint*)(a3 + 4);

            // DirectorUpdate Category
            if (category == 0x6D) {
                switch (type) {
                    // Duty Commenced
                    case 0x40000001:
                        dutyStarted = true;
                        break;

                    // Party Wipe
                    case 0x40000005:
                        dutyStarted = false;
                        break;

                    // Duty Recommence
                    case 0x40000006:
                        dutyStarted = true;
                        break;

                    // Duty Completed
                    case 0x40000003:
                        dutyStarted            = false;
                        completedThisTerritory = true;
                        break;
                }
            }
        } catch (Exception ex) {
            PluginLog.Error(ex, "Failed to get Duty Started Status");
        }

        return dutyEventHook!.Original(a1, a2, a3);
    }

    private void TerritoryChanged(object? sender, ushort e)
    {
        if (dutyStarted) {
            dutyStarted = false;
        }

        completedThisTerritory = false;
    }

    private bool BoundByDuty()
    {
        return Condition![ConditionFlag.BoundByDuty] || Condition[ConditionFlag.BoundByDuty56] || Condition[ConditionFlag.BoundByDuty95];
    }

    public void Dispose()
    {
        if (Framework is not null) Framework.Update               -= FrameworkUpdate;
        if (ClientState is not null) ClientState.TerritoryChanged -= TerritoryChanged;
    }

    private class PartyMemberSerializer : JsonConverter<PartyMember>
    {
        public override PartyMember Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, PartyMember value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", value.Name.ToString());
            writer.WriteString("Job",  ((Job)value.ClassJob.Id).ToString());
            writer.WriteEndObject();
        }
    }
}
