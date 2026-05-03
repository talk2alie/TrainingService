using FluentValidation;
using Training.Api.Requests;

namespace Training.Api.Validators;

public sealed class UpdateSessionNoteRequestValidator : AbstractValidator<UpdateSessionNoteRequest>
{
    public UpdateSessionNoteRequestValidator()
    {
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
    }
}
