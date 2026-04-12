using FluentValidation;
using Training.Api.Requests;

namespace Training.Api.Validators;

public sealed class SyncTrainingRequestValidator : AbstractValidator<SyncTrainingRequest>
{
    public SyncTrainingRequestValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.SyncedAtUtc)
            .NotNull();
    }
}
