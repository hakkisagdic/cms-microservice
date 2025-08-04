using FluentValidation;
using IdentityService.Core.DTOs.User;

namespace IdentityService.Application.Validators;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(500)
            .Must(BeValidUrl)
            .WithMessage("Profile image URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl));
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp);
    }
}
