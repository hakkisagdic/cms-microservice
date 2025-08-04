using FluentValidation;
using UserService.Core.Features.Users.Commands;

namespace UserService.Core.Validators;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.User)
            .NotNull()
            .WithMessage("User data is required.")
            .SetValidator(new UpdateUserDtoValidator());
    }
}
