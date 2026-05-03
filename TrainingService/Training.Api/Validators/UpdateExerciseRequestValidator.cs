using FluentValidation;
using Training.Api.Requests;

namespace Training.Api.Validators;

public sealed class UpdateExerciseRequestValidator : AbstractValidator<UpdateExerciseRequest>
{
    public UpdateExerciseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.RequiredEquipment).NotNull().Must(x => x.Count > 0);
        RuleFor(x => x.PrimaryMuscleGroup).NotEmpty().MaximumLength(120);
    }
}
