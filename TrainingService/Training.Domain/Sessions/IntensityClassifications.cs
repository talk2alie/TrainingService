namespace Training.Domain.Sessions;

/// <summary>
/// Represents Rate of Perceived Exertion (RPE) for strength training,
/// expressed as proximity to failure. Higher values indicate fewer
/// reps left in reserve (RIR) at the end of the set.
/// </summary>
public enum RpeScale
{
    /// <summary>
    /// RPE 6 — Approximately 4 reps left in reserve (RIR).
    /// Light effort; far from failure; warm-up or technique work.
    /// </summary>
    Rpe6 = 6,

    /// <summary>
    /// RPE 7 — Approximately 3 reps left in reserve (RIR).
    /// Moderate effort; sustainable; good for volume accumulation.
    /// </summary>
    Rpe7 = 7,

    /// <summary>
    /// RPE 8 — Approximately 2 reps left in reserve (RIR).
    /// Hard effort; challenging but repeatable; primary working sets.
    /// </summary>
    Rpe8 = 8,

    /// <summary>
    /// RPE 9 — Approximately 1 rep left in reserve (RIR).
    /// Very hard effort; near failure; used for top sets or testing.
    /// </summary>
    Rpe9 = 9,

    /// <summary>
    /// RPE 10 — Zero reps left in reserve (RIR).
    /// Maximal effort; failure or very close to it; cannot repeat.
    /// </summary>
    Rpe10 = 10
}


public static class RpeScaleExtensions
{
    /// <summary>
    /// Gets a human-readable description of the RPE value,
    /// based on reps left in reserve (RIR).
    /// </summary>
    public static string Description(this RpeScale rpe) => rpe switch
    {
        RpeScale.Rpe6 => "RPE 6 — ~4 reps in reserve (light, comfortable).",
        RpeScale.Rpe7 => "RPE 7 — ~3 reps in reserve (moderate, sustainable).",
        RpeScale.Rpe8 => "RPE 8 — ~2 reps in reserve (hard, working set).",
        RpeScale.Rpe9 => "RPE 9 — ~1 rep in reserve (very hard, near failure).",
        RpeScale.Rpe10 => "RPE 10 — 0 reps in reserve (max effort).",
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
