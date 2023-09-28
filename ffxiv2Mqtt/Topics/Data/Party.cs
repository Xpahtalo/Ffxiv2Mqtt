using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Enums;

namespace Ffxiv2Mqtt.Topics.Data;

internal class Party : Topic, IDisposable
{
    private          int                   partyCount;
    private readonly JsonSerializerOptions serializerOptions;

    protected override string TopicPath => "Party";
    protected override bool   Retained  => false;


    public Party()
    {
        serializerOptions = new JsonSerializerOptions
                            {
                                Converters = { new PartyMemberSerializer() },
                            };
        Service.Framework.Update += FrameworkUpdate;
    }

    private void FrameworkUpdate(IFramework framework)
    {
        if (Service.DutyState is { IsDutyStarted: false })
            return;

        // Assuming none of the previous blocking conditions are true, send party messages if the count has changed.
        if (Service.PartyList.Length == partyCount)
            return;

        partyCount = Service.PartyList.Length;
        Publish($"{TopicPath}/Count",   partyCount.ToString());
        Publish($"{TopicPath}/Members", JsonSerializer.Serialize(Service.PartyList, serializerOptions));
    }

    public void Dispose() { Service.Framework.Update -= FrameworkUpdate; }

    private class PartyMemberSerializer : JsonConverter<PartyMember>
    {
        public override PartyMember Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) { throw new NotImplementedException(); }

        public override void Write(Utf8JsonWriter writer, PartyMember value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", value.Name.ToString());
            writer.WriteString("Job",  ((Job)value.ClassJob.Id).ToString());
            writer.WriteEndObject();
        }
    }
}
