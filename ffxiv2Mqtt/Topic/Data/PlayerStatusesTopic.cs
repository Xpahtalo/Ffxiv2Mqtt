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

        public PlayerStatusesTopic(MqttManager m) : base(m)
        {
            topic = "Player/Status";
        }

        public void Update()
        {
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is not null)
            {
                var statusList = from status in localPlayer.StatusList
                                    where status.Address != IntPtr.Zero
                                    select status;

                TestValue(statusList.Count(), ref statusCount);
                
                
            }

            PublishIfNeeded();
        }

        internal override void Publish()
        {
            var serializeOptions = new JsonSerializerOptions { Converters = { new StatusSerializer() } };
            mqttManager.PublishMessage(topic, JsonSerializer.Serialize(DalamudServices.ClientState.LocalPlayer.StatusList as IReadOnlyCollection<Status>, serializeOptions));
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
