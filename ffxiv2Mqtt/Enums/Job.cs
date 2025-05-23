﻿namespace Ffxiv2Mqtt.Enums;

public enum Job : uint
{
    Unknown       = 0,
    Gladiator     = 1,
    Pugilist      = 2,
    Marauder      = 3,
    Lancer        = 4,
    Archer        = 5,
    Conjurer      = 6,
    Thaumaturge   = 7,
    Carpenter     = 8,
    Blacksmith    = 9,
    Armorer       = 10,
    Goldsmith     = 11,
    Leatherworker = 12,
    Weaver        = 13,
    Alchemist     = 14,
    Culinarian    = 15,
    Miner         = 16,
    Botanist      = 17,
    Fisher        = 18,
    Paladin       = 19,
    Monk          = 20,
    Warrior       = 21,
    Dragoon       = 22,
    Bard          = 23,
    WhiteMage     = 24,
    BlackMage     = 25,
    Arcanist      = 26,
    Summoner      = 27,
    Scholar       = 28,
    Rogue         = 29,
    Ninja         = 30,
    Machinist     = 31,
    DarkKnight    = 32,
    Astrologian   = 33,
    Samurai       = 34,
    RedMage       = 35,
    BlueMage      = 36,
    Gunbreaker    = 37,
    Dancer        = 38,
    Reaper        = 39,
    Sage          = 40,
    Viper         = 41,
    Pictomancer   = 42,
}

public static class JobExtensions
{
    public static bool IsGatherer(this Job job)
    {
        return job switch
        {
            Job.Miner    => true,
            Job.Botanist => true,
            Job.Fisher   => true,
            _            => false,
        };
    }

    public static bool IsCrafter(this Job job)
    {
        return job switch
        {
            Job.Carpenter     => true,
            Job.Blacksmith    => true,
            Job.Armorer       => true,
            Job.Goldsmith     => true,
            Job.Leatherworker => true,
            Job.Weaver        => true,
            Job.Alchemist     => true,
            Job.Culinarian    => true,
            _                 => false,
        };
    }

    public static Job ToJob(this uint? jobId)
    {
        if (jobId is null) return Job.Unknown;
        if (typeof(Job).IsEnumDefined(jobId)) return (Job)jobId;
        return Job.Unknown;
    }
}
