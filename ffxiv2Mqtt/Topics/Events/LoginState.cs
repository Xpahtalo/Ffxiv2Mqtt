using System;
using System.Text.Json;
using System.Threading.Tasks;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Events;

internal sealed class LoginState : Topic, ICleanable, IDisposable
{
    private string characterName = "";

    protected override string TopicPath => "Event/Login";
    protected override bool   Retained  => true;

    // ReSharper disable once MemberCanBePrivate.Global
    [PluginService] public IClientState? ClientState { get; set; }

    public override void Initialize()
    {
        ClientState!.Login  += Login;
        ClientState!.Logout += Logout;
    }

    // Publish the login state and character name when logging in.
    private void Login()
    {
        Task.Run(async () =>
        {
            while (ClientState?.LocalPlayer?.Name is null)
                await Task.Delay(1000);
            characterName = ClientState!.LocalPlayer!.Name.ToString();
            Publish(JsonSerializer.Serialize(new
                                             {
                                                 LoggedIn  = true,
                                                 Character = characterName,
                                             }));
        });
    }

    // Publish the login state and last character name when logging out.
    private void Logout()
    {
        Publish(JsonSerializer.Serialize(new
                                         {
                                             LoggedIn  = false,
                                             Character = characterName,
                                         }));
    }

    // Clear out the retained message when exiting.
    public void Cleanup()
    {
        Publish("");
    }

    public void Dispose()
    {
        ClientState!.Login  -= Login;
        ClientState!.Logout -= Logout;
    }
}
