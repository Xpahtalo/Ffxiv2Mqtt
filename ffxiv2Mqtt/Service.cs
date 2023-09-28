using System.Diagnostics.CodeAnalysis;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt;

#pragma warning disable CS8618
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public class Service
{
    internal static               PlayerEvents           PlayerEvents;
    internal static               MqttManager            MqttManager;
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static IChatGui               ChatGui         { get; private set; }
    [PluginService] public static IToastGui              ToastGui        { get; private set; }
    [PluginService] public static IClientState           ClientState     { get; private set; }
    [PluginService] public static IPartyList             PartyList       { get; private set; }
    [PluginService] public static ICommandManager        CommandManager  { get; private set; }
    [PluginService] public static IDataManager           DataManager     { get; private set; }
    [PluginService] public static IFramework             Framework       { get; private set; }
    [PluginService] public static IObjectTable           ObjectTable     { get; private set; }
    [PluginService] public static IGameGui               GameGui         { get; private set; }
    [PluginService] public static IDutyState             DutyState       { get; private set; }
    [PluginService] public static IJobGauges             JobGauges       { get; private set; }
    [PluginService] public static ICondition             Condition       { get; private set; }
    [PluginService] public static IPluginLog             Log             { get; private set; }
}
#pragma warning restore CS8618
