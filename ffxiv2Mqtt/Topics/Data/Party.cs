using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Party;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Ffxiv2Mqtt.Topics.Data;

internal class Party : Topic, IDisposable
{
    private          int                   partyCount;
    private readonly JsonSerializerOptions serializerOptions;
    
    protected override string TopicPath => "Party";
    protected override bool   Retained  => false;

    [PluginService] public PartyList? PartyList { get; set; }
    [PluginService] public Framework? Framework { get; set; }
    [PluginService] public Condition? Condition { get; set; }

    public Party()
    {
        serializerOptions = new JsonSerializerOptions
                            {
                                Converters = { new PartyMemberSerializer() },
                            };
    }

    public override void Initialize()
    {
        if (Framework is not null) Framework.Update += CheckPartyChanges;
    }

    private void CheckPartyChanges(Framework framework)
    {
        if (Condition is not null && (Condition[ConditionFlag.BetweenAreas] || Condition[ConditionFlag.BetweenAreas51]))
            return; // The party list is empty when transitioning, so block all changes.
        
        if (PartyList!.Length == partyCount)
            return;

        partyCount = PartyList.Length;
        Publish($"{TopicPath}/Count",   partyCount.ToString());
        Publish($"{TopicPath}/Members", JsonSerializer.Serialize(PartyList, serializerOptions));
    }

    public void Dispose()
    {
        if (Framework is not null) Framework.Update -= CheckPartyChanges;
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
            writer.WriteString("Job", ((Job)value.ClassJob.Id).ToString());
            writer.WriteEndObject();
        }
    }
}
