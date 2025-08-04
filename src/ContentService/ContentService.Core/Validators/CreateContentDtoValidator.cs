using FluentValidation;
using ContentService.Core.DTOs;

namespace ContentService.Core.Validators;

public class CreateContentDtoValidator : AbstractValidator<CreateContentDto>
{
    public CreateContentDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Body is required.")
            .MaximumLength(10000)
            .WithMessage("Body cannot exceed 10000 characters.");

        RuleFor(x => x.Summary)
            .MaximumLength(500)
            .WithMessage("Summary cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Summary));

        RuleFor(x => x.MetaTitle)
            .MaximumLength(100)
            .WithMessage("Meta title cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.MetaTitle));

        RuleFor(x => x.MetaDescription)
            .MaximumLength(300)
            .WithMessage("Meta description cannot exceed 300 characters.")
            .When(x => !string.IsNullOrEmpty(x.MetaDescription));

        RuleFor(x => x.Tags)
            .MaximumLength(500)
            .WithMessage("Tags cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Tags));

        RuleFor(x => x.Category)
            .MaximumLength(100)
            .WithMessage("Category cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.Slug)
            .MaximumLength(200)
            .WithMessage("Slug cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.AuthorId)
            .NotEmpty()
            .WithMessage("Author ID is required.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sort order must be greater than or equal to 0.");
    }
}
