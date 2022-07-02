using Ffxiv2Mqtt.Topic.Interfaces;
using Dalamud.Game.ClientState.Statuses;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Linq;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class PlayerStatusesTopic : Topic, IUpdatable
    {
        public StatusList Statuses { get => DalamudServices.ClientState.LocalPlayer.StatusList; }

        int statusCount;
        JsonSerializerOptions serializerOptions;

        public PlayerStatusesTopic(MqttManager m) : base(m)
        {
            topic = "Player/Status";
            serializerOptions = new JsonSerializerOptions
            {
                Converters = { new StatusSerializer() }
            };
        }

        public void Update()
        {
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is not null)
            {
                var activeStatusCount = localPlayer.StatusList.Count<Status>(status => status.Address != IntPtr.Zero);
                TestValue(activeStatusCount, ref statusCount);
            }

            PublishIfNeeded();
        }

        internal override void PublishIfNeeded(bool retained = false)
        {
            if (needsPublishing)
            {
                Publish();
                needsPublishing = false;
            }
        }
        
        internal override void Publish()
        {
            mqttManager.PublishMessage(topic, JsonSerializer.Serialize(DalamudServices.ClientState.LocalPlayer.StatusList as IReadOnlyCollection<Status>, serializerOptions));
        }
        
        public class StatusSerializer:JsonConverter<Status>
        {
            public override Status? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
            public override void Write(Utf8JsonWriter writer, Status value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString("Name", value.GameData.Name.ToString());
                writer.WriteNumber("StatusId", value.StatusId);
                writer.WriteNumber("Param", value.Param);
                writer.WriteNumber("RemainingTime", value.RemainingTime);
                writer.WriteNumber("SourceID", value.SourceID);
                writer.WriteNumber("StackCount", value.StackCount);
                writer.WriteEndObject();
            }
        }
    }
}
