# Copilot Instructions

## Project Guidelines
- The project should keep logging extensible so it can later send logs to Azure Application Insights and/or a database sink.
- Prefer registering custom middleware via extension methods in Program.cs for consistency (e.g., CorrelationId like GlobalException/Serilog setups).
- Ensure physical directory names align with namespaces to avoid mismatches in Visual Studio.
- Review code against the domain model after namespace correction; maintain alignment with the intended domain model during follow-up edits.

## API Contract Guidelines
- Use Request/Response suffixes for API/application contract types instead of the Dto suffix.

## Domain Model Guidelines
- Model `MuscleGroup` as an immutable value object within its bounded context namespace `GymPal.Domain.Exercises` with the following attributes:
  - `Name`
  - `Description`
  - `Thumbnail` 
- Ensure that `MuscleGroup` has no Id or lifecycle attributes.
- In Exercise metadata, model muscle targeting with `PrimaryMuscleGroup` and `SecondaryMuscleGroups` instead of a single `MuscleGroups` collection.
- If more value objects require case-insensitive equality, refactor to a shared reusable mechanism instead of repeating per type.

## Exercise Handling Guidelines
- For Exercise alias handling, enforce case-insensitive equality semantics if `ExerciseName` does not already provide it.