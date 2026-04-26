using Training.Domain.Common;

namespace Training.Domain.Sessions;

/// <summary>
/// Contract for heart rate zone calculations.
/// </summary>
public interface IHeartRateZoneService
{
    /// <summary>
    /// Calculates the current heart rate zone.
    /// </summary>
    HeartRateZone Calculate(HeartRate hr, int age, int? restingHr = null, int? maxHr = null);
}

/// <summary>
/// Injectable domain service for heart rate zone calculations.
/// </summary>
public sealed class HeartRateZoneService : IHeartRateZoneService
{
    /// <summary>
    /// Calculates the current heart rate zone.
    /// </summary>
    public HeartRateZone Calculate(HeartRate hr, int age, int? restingHr = null, int? maxHr = null)
    {
        ArgumentNullException.ThrowIfNull(hr);
        Guard.AgainstOutOfRange(age, 1, 120, nameof(age));

        var effectiveMaxHr = maxHr ?? 220 - age;
        Guard.AgainstOutOfRange(effectiveMaxHr, 30, 240, nameof(maxHr));

        if (restingHr.HasValue)
        {
            Guard.AgainstOutOfRange(restingHr.Value, 30, effectiveMaxHr - 1, nameof(restingHr));

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
