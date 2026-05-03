using FluentValidation;
using Training.Api.Requests;

namespace Training.Api.Validators;

public sealed class CreateRoutineRequestValidator : AbstractValidator<CreateRoutineRequest>
{
    public CreateRoutineRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}
