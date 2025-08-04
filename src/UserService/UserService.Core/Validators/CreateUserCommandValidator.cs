using FluentValidation;
using UserService.Core.Features.Users.Commands;

namespace UserService.Core.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.User)
            .NotNull()
            .WithMessage("User data is required.")
            .SetValidator(new CreateUserDtoValidator());
    }
}
