using Ffxiv2Mqtt.TopicTracker.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Ffxiv2Mqtt.TopicTracker.Events
{
    internal class LoginTopic : Topic, ICleanable
    {
        public bool LoggedIn { get; set; }
        public string LoggedInCharacter { get; set; }

        internal LoginTopic(MqttManager m) : base(m)
        {
            topic = "Event/Login";
            LoggedInCharacter = "";
            Dalamud.ClientState.Login += Login;
            Dalamud.ClientState.Logout += Logout;
        }

        private void Login(object? s, System.EventArgs e)
        {
            LoggedIn = true;

            Task.Run(() =>
            {
                var name = Dalamud.ClientState?.LocalPlayer?.Name?.ToString();
                while (name is null || name == "")
                    Thread.Sleep(1000);
                LoggedInCharacter = name;
            });
            Publish(true);
        }

        private void Logout(object? s, System.EventArgs e)
        {
            LoggedIn = false;
            LoggedInCharacter = "";
            Publish();
        }

        public void Cleanup()
        {
            mqttManager.PublishMessage(topic, "");
            Dalamud.ClientState.Login -= Login;
            Dalamud.ClientState.Logout -= Logout;
        }

    }
}
