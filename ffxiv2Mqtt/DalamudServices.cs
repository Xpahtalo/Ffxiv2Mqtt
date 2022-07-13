using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace Ffxiv2Mqtt;

// This is used to skip the usual service DI throughout the plugin. Technically incorrect, but significantly easier until the DI is improved.

// From GatherBuddy https://github.com/Ottermandias/GatherBuddy
public class DalamudServices
{
    [PluginService] [RequiredVersion("1.0")]
    public static DalamudPluginInterface PluginInterface { get; } = null!;

    [PluginService] [RequiredVersion("1.0")]
    public static CommandManager CommandManager { get; } = null!;

    //[PluginService][RequiredVersion("1.0")] public static SigScanner SigScanner { get; private set; } = null!;
    [PluginService] [RequiredVersion("1.0")]
    public static DataManager GameData { get; } = null!;

    [PluginService] [RequiredVersion("1.0")]
    public static ClientState ClientState { get; } = null!;

    //[PluginService][RequiredVersion("1.0")] public static ChatGui Chat { get; private set; } = null!;
    [PluginService] [RequiredVersion("1.0")]
    public static Framework Framework { get; } = null!;

    [PluginService] [RequiredVersion("1.0")]
    public static Condition Conditions { get; } = null!;

    //[PluginService][RequiredVersion("1.0")] public static TargetManager Targets { get; private set; } = null!;
    //[PluginService][RequiredVersion("1.0")] public static ObjectTable Objects { get; private set; } = null!;
    //[PluginService][RequiredVersion("1.0")] public static TitleScreenMenu TitleScreenMenu { get; private set; } = null!;
    [PluginService] [RequiredVersion("1.0")]
    public static GameGui GameGui { get; } = null!;

    [PluginService] [RequiredVersion("1.0")]
    public static JobGauges JobGauges { get; } = null!;

    public static void Initialize(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<DalamudServices>();
    }
}
