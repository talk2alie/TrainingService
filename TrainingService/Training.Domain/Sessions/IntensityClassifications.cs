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

/// <summary>
/// Calculates heart rate zones from heart rate measurements and user profile data.
/// </summary>
public sealed class HeartRateZoneService
{
    /// <summary>
    /// Calculates the current heart rate zone.
    /// </summary>
    public HeartRateZone Calculate(Training.Domain.Common.HeartRate hr, int age, int? restingHr = null, int? maxHr = null)
    {
        ArgumentNullException.ThrowIfNull(hr);
        Training.Domain.Common.Guard.AgainstOutOfRange(age, 1, 120, nameof(age));

        var effectiveMaxHr = maxHr ?? 220 - age;
        Training.Domain.Common.Guard.AgainstOutOfRange(effectiveMaxHr, 30, 240, nameof(maxHr));

        if (restingHr.HasValue)
        {
            Training.Domain.Common.Guard.AgainstOutOfRange(restingHr.Value, 30, effectiveMaxHr - 1, nameof(restingHr));

            var hrr = effectiveMaxHr - restingHr.Value;
            if (hrr <= 0)
            {
                throw new ArgumentException("Heart rate reserve must be positive.", nameof(restingHr));
            }

            var intensity = (double)(hr.BeatsPerMinute - restingHr.Value) / hrr;
            return intensity switch
            {
                <= 0.60 => HeartRateZone.Recovery,
                <= 0.70 => HeartRateZone.AerobicBase,
                <= 0.80 => HeartRateZone.Tempo,
                <= 0.90 => HeartRateZone.Threshold,
                _ => HeartRateZone.Vo2Max
            };
        }

        var percentageOfMax = (double)hr.BeatsPerMinute / effectiveMaxHr;
        return percentageOfMax switch
        {
            < 0.60 => HeartRateZone.Recovery,
            < 0.70 => HeartRateZone.AerobicBase,
            < 0.80 => HeartRateZone.Tempo,
            < 0.90 => HeartRateZone.Threshold,
            _ => HeartRateZone.Vo2Max
        };
    }
}
