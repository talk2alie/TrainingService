# Domain Model Resolution

## Summary
Implemented an initial pure-domain model for the training microservice using DDD-oriented aggregates, entities, value objects, and enums under `GymPal.Domain.[Context]` namespaces.

## Added Types
- Aggregates: `Routine`, `Session`, `Exercise`, `MuscleGroup`
- Entities: `RoutineExercise`, `SessionExercise`
- Value Objects: `PlannedSet`, `LoggedSet`, `Weight`, `Duration`, `Distance`, `RpeScale`, `HeartRate`, `Order`, `ExerciseName`, `RoutineName`, `MuscleGroupName`, `ExerciseDescription`, `Hyperlink`, `SessionNote`, `LogNote`, `ImageData`, `MuscleGroupDescription`
- Enums: `DifficultyLevel`, `EquipmentType`, `ExerciseCategory`, `ImageFormat`
- Shared validation helper: `Guard`

## Domain Constraints Applied
- Immutable VOs with value equality.
- Factory methods and private constructors for VOs.
- Entity and aggregate invariants enforced via guard clauses.
- Aggregate collections exposed as `IReadOnlyList<T>`.
- No persistence concerns or EF Core attributes.

## Notes
- Existing solution targets `.NET 10` at workspace level. The new domain model is implemented with C# 12-compatible patterns and remains pure domain; no target framework changes were made to avoid cross-project disruption.
- Logging setup is API-layer concern in this solution and did not require domain-layer changes for this story.
