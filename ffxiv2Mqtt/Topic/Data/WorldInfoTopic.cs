using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class WorldInfoTopic : Topic,IUpdatable
    {
        uint currentWorldId = 0;
        
        public WorldInfoTopic(MqttManager m):base(m)
        {
            topic = "Event/WorldChanged";
        }
        
        public void Update()
        {
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is not null)
            {
                var currentWorld = localPlayer.CurrentWorld;
                var homeWorld = localPlayer.HomeWorld;

                if (currentWorldId != currentWorld.Id)
                {
                    currentWorldId = currentWorld.Id;
                    
                    Publish(new
                    {
                        CurrentWorld = currentWorld.GameData.Name.ToString(),
                        CurrentWorldId = currentWorld.Id,
                        HomeWorld = homeWorld.GameData.Name.ToString(),
                        HomeWorldId = homeWorld.Id
                    });
                }
            }
        }
    }
}
