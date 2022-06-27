using Ffxiv2Mqtt.TopicTracker.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Ffxiv2Mqtt.TopicTracker.Events
{
    internal class LoginTopic : Topic, ICleanable
    {
        public bool LoggedIn { get; set; }
        public string Character { get; set; }

        internal LoginTopic(MqttManager m) : base(m)
        {
            topic = "Event/Login";
            Character = "";
            Dalamud.ClientState.Login += Login;
            Dalamud.ClientState.Logout += Logout;
        }

        private void Login(object? s, System.EventArgs e)
        {
            LoggedIn = true;

            Task.Run(() =>
            {
                while (Dalamud.ClientState?.LocalPlayer?.Name is null)
                    Thread.Sleep(1000);
                Character = Dalamud.ClientState.LocalPlayer.Name.ToString();
                Publish(true);
            });
        }

        private void Logout(object? s, System.EventArgs e)
        {
            LoggedIn = false;
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
