using System;
using System.Text.Json;
using System.Threading.Tasks;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Events;

internal sealed class LoginState : Topic, ICleanable, IDisposable
{
    private string characterName = "";

    protected override string TopicPath => "Event/Login";
    protected override bool   Retained  => true;

    // ReSharper disable once MemberCanBePrivate.Global

    public LoginState()
    {
        Service.ClientState.Login  += Login;
        Service.ClientState.Logout += Logout;
    }

    // Publish the login state and character name when logging in.
    private void Login()
    {
        Task.Run(async void () =>
        {
            try
            {
                while (!Service.PlayerState.IsLoaded)
                    await Task.Delay(1000);
                characterName = Service.PlayerState.CharacterName;
                Publish(JsonSerializer.Serialize(new
                {
                    LoggedIn  = true,
                    Character = characterName,
                }));
            }
            catch (Exception e)
            {
                Service.Log.Error($"Exception while waiting for character to load:\n{e}");
            }
        });
    }

    // Publish the login state and last character name when logging out.
    private void Logout(int type, int code)
    {
        Publish(JsonSerializer.Serialize(new
                                         {
                                             LoggedIn  = false,
                                             Character = characterName,
                                         }));
    }

    // Clear out the retained message when exiting.
    public void Cleanup() { Publish(""); }

    public void Dispose()
    {
        Service.ClientState.Login  -= Login;
        Service.ClientState.Logout -= Logout;
    }
}
