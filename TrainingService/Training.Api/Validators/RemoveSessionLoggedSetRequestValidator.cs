using FluentValidation;
using Training.Api.Requests;

namespace Training.Api.Validators;

public sealed class RemoveSessionLoggedSetRequestValidator : AbstractValidator<RemoveSessionLoggedSetRequest>
{
    public RemoveSessionLoggedSetRequestValidator()
    {
        RuleFor(x => x.SetOrder).GreaterThan(0);
    }
}
