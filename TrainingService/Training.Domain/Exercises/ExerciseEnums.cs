namespace Training.Domain.Exercises;

/// <summary>
/// Represents the relative difficulty of an exercise.
/// </summary>
public enum DifficultyLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3
}

/// <summary>
/// Classifies exercises by their primary training intent.
/// This is a functional programming category (not anatomical),
/// used for filtering, routine building, analytics, and UI grouping.
/// </summary>
public enum ExerciseCategory
{
    /// <summary>
    /// Strength-focused movements intended to improve maximal force output.
    /// Examples: deadlift, squat, bench press.
    /// Typically lower reps, higher load, longer rest.
    /// </summary>
    Strength = 1,

    /// <summary>
    /// Hypertrophy-focused movements intended to increase muscle size.
    /// Examples: bicep curls, lateral raises, leg extensions.
    /// Typically moderate reps, moderate load, moderate rest.
    /// </summary>
    Hypertrophy = 2,

    /// <summary>
    /// Endurance-focused movements intended to improve muscular or aerobic stamina.
    /// Examples: high-rep bodyweight work, long-duration carries, tempo circuits.
    /// </summary>
    Endurance = 3,

    /// <summary>
    /// Mobility-focused movements intended to improve joint range of motion,
    /// control, and movement quality. Examples: hip openers, thoracic rotations.
    /// </summary>
    Mobility = 4,

    /// <summary>
    /// Cardio-focused movements intended to elevate heart rate for sustained periods.
    /// Examples: treadmill running, rowing, cycling, jump rope.
    /// </summary>
    Cardio = 5,

    /// <summary>
    /// Recovery-focused movements intended to reduce fatigue, promote blood flow,
    /// and support active recovery. Examples: light stretching, walking, easy cycling.
    /// </summary>
    Recovery = 6
}

/// <summary>
/// Classifies exercises by primary biomechanical movement pattern.
/// Movement patterns describe how the body moves, not which muscles are used.
/// </summary>
public enum MovementPattern
{
    /// <summary>
    /// Upper-body pushing pattern involving horizontal or vertical force production
    /// (e.g., bench press, overhead press, push-up).
    /// </summary>
    Push = 1,

    /// <summary>
    /// Upper-body pulling pattern involving horizontal or vertical force production
    /// (e.g., pull-up, row, lat pulldown).
    /// </summary>
    Pull = 2,

    /// <summary>
    /// Lower-body knee-dominant pattern emphasizing upright torso and knee flexion/extension
    /// (e.g., squat variations, leg press).
    /// </summary>
    Squat = 3,

    /// <summary>
    /// Lower-body hip-dominant pattern emphasizing hip flexion/extension
    /// (e.g., deadlift, RDL, hip thrust).
    /// </summary>
    Hinge = 4,

    /// <summary>
    /// Core bracing and stabilization against spinal movement
    /// (e.g., plank, hollow hold, farmer carry).
    /// </summary>
    Core = 5,

    /// <summary>
    /// Rotational or anti-rotational trunk control
    /// (e.g., woodchop, Pallof press, cable rotation).
    /// </summary>
    Rotation = 6,

    /// <summary>
    /// Locomotion or gait-based movement requiring coordinated lower-body and core control
    /// (e.g., walking, running, sled push, loaded carries).
    /// </summary>
    Locomotion = 7,

    /// <summary>
    /// Uncategorized or mixed-pattern movement that does not fit cleanly into a single pattern.
    /// </summary>
    Other = 99
}


/// <summary>
/// Defines the primary equipment required to perform an exercise.
/// </summary>
public enum EquipmentType
{
    None = 0,
    Bodyweight = 1,
    Dumbbell = 2,
    Barbell = 3,
    Kettlebell = 4,
    Machine = 5,
    Cable = 6,
    ResistanceBand = 7,
    CardioMachine = 8,
    Other = 9
}
