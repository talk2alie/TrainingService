using FluentValidation;
using Training.Api.Requests;

namespace Training.Api.Validators;

public sealed class AddSessionLoggedSetRequestValidator : AbstractValidator<AddSessionLoggedSetRequest>
{
    public AddSessionLoggedSetRequestValidator()
    {
        RuleFor(x => x.Rpe).IsInEnum();

        RuleFor(x => x.Repetitions)
            .GreaterThan(0)
            .When(x => x.Repetitions.HasValue);

        RuleFor(x => x.WeightKg)
            .GreaterThan(0)
            .When(x => x.WeightKg.HasValue);

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0)
            .When(x => x.DurationSeconds.HasValue);

        RuleFor(x => x.DistanceMeters)
            .GreaterThan(0)
            .When(x => x.DistanceMeters.HasValue);

        RuleFor(x => x.HeartRateBpm)
            .GreaterThan(0)
            .When(x => x.HeartRateBpm.HasValue);

        RuleFor(x => x)
            .Must(request =>
                request.Repetitions.HasValue ||
                request.WeightKg.HasValue ||
                request.DurationSeconds.HasValue ||
                request.DistanceMeters.HasValue ||
                request.HeartRateBpm.HasValue)
            .WithMessage("At least one metric must be provided.");
    }
}
