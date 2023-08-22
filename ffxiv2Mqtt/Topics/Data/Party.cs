using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.DutyState;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;

// Adapted from NoTankYou (https://github.com/MidoriKami/NoTankYou)
// https://github.com/MidoriKami/NoTankYou/blob/master/NoTankYou/System/DutyEventManager.cs

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Ffxiv2Mqtt.Topics.Data;

internal class Party : Topic, IDisposable
{
    private          int                   partyCount;
    private readonly JsonSerializerOptions serializerOptions;
    
    protected override string TopicPath => "Party";
    protected override bool   Retained  => false;

    [PluginService] public PartyList?   PartyList   { get; set; }
    [PluginService] public Framework?   Framework   { get; set; }
    [PluginService] public Condition?   Condition   { get; set; }
    [PluginService] public DutyState?   DutyState   { get; set; }

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
    }

    private void FrameworkUpdate(Framework framework)
    {
        if (DutyState is { IsDutyStarted: false })
            return;
        
        // Assuming none of the previous blocking conditions are true, send party messages if the count has changed.
        if(PartyList is not null && PartyList.Length == partyCount)    
            return;

        partyCount = PartyList!.Length;
        Publish($"{TopicPath}/Count",   partyCount.ToString());
        Publish($"{TopicPath}/Members", JsonSerializer.Serialize(PartyList, serializerOptions));
    }

    public void Dispose()
    {
        if (Framework is not null) Framework.Update               -= FrameworkUpdate;
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
