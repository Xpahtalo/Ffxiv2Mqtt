using Dalamud.Game.ClientState.Objects.Types;
using Ffxiv2Mqtt.Enums;

namespace Ffxiv2Mqtt.Extensions;

public static class CharacterExtensions {
    public static bool IsJob(this ICharacter character, Job job) {
        if (!character.ClassJob.IsValid)
            return false;
        return (Job)character.ClassJob.RowId == job;
    }
}