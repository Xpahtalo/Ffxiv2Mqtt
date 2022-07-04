using System;
using System.Threading;
using System.Threading.Tasks;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Events
{
    internal sealed class LoginTopic : Topic, ICleanable, IDisposable
    {
        public bool LoggedIn { get; set; }
        public string Character { get; set; }

        internal LoginTopic(MqttManager m) : base(m)
        {
            topic = "Event/Login";
            LoggedIn = DalamudServices.ClientState.IsLoggedIn;
            Character = DalamudServices.ClientState.LocalPlayer?.Name.ToString() ?? ""; 
            DalamudServices.ClientState.Login += Login;
            DalamudServices.ClientState.Logout += Logout;

            Publish(true);
        }

        private void Login(object? s, System.EventArgs e)
        {
            LoggedIn = true;

            Task.Run(() =>
            {
                while (DalamudServices.ClientState?.LocalPlayer?.Name is null)
                    Thread.Sleep(1000);
                Character = DalamudServices.ClientState.LocalPlayer.Name.ToString();
                Publish(true);
            });
        }
        
        private void Logout(object? s, System.EventArgs e)
        {
            LoggedIn = false;
            Publish(true);
        }

        public void Cleanup()
        {
            mqttManager.PublishMessage(topic, "");
        }

        public void Dispose()
        {
            DalamudServices.ClientState.Login -= Login;
            DalamudServices.ClientState.Logout -= Logout;
        }

    }
}
