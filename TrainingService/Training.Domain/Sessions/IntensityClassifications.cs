using Training.Domain.Common;

namespace Training.Domain.Sessions;

/// <summary>
/// Represents subjective rate of perceived exertion.
/// </summary>
public enum RpeScale
{
    Moderate = 6,
    SomewhatHard = 7,
    Hard = 8,
    VeryHard = 9,
    MaxEffort = 10
}

/// <summary>
/// Provides helpers for <see cref="RpeScale"/>.
/// </summary>
public static class RpeScaleExtensions
{
    /// <summary>
    /// Gets a human-readable description of the RPE value.
    /// </summary>
    public static string Description(this RpeScale rpe) => rpe switch
    {
        RpeScale.Moderate => "Moderate effort",
        RpeScale.SomewhatHard => "Somewhat hard effort",
        RpeScale.Hard => "Hard effort",
        RpeScale.VeryHard => "Very hard effort",
        RpeScale.MaxEffort => "Maximum effort",
        _ => throw new ArgumentOutOfRangeException(nameof(rpe))
    };
}

/// <summary>
/// Represents physiology-based heart rate zones.
/// </summary>
public enum HeartRateZone
{
    Recovery = 1,
    AerobicBase = 2,
    Tempo = 3,
    Threshold = 4,
    Vo2Max = 5
}

/// <summary>
/// Provides helpers for <see cref="HeartRateZone"/>.
/// </summary>
public static class HeartRateZoneExtensions
{
    /// <summary>
    /// Gets a human-readable description of the heart rate zone.
    /// </summary>
    public static string Description(this HeartRateZone zone) => zone switch
    {
        HeartRateZone.Recovery =>
            "Very low intensity for recovery and regeneration.",
        HeartRateZone.AerobicBase =>
            "Low intensity for fat oxidation and aerobic base building.",
        HeartRateZone.Tempo =>
            "Moderate intensity for tempo and aerobic power.",
        HeartRateZone.Threshold =>
            "High intensity near lactate threshold.",
        HeartRateZone.Vo2Max =>
            "Very high intensity for VO₂ max development.",
        _ => throw new ArgumentOutOfRangeException(nameof(zone))
    };
}
