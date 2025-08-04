using FluentAssertions;
using FluentValidation.TestHelper;
using ContentService.Core.DTOs;
using ContentService.Core.Entities;
using ContentService.Core.Validators;
using Xunit;

namespace ContentService.Tests.Unit;

public class CreateContentDtoValidatorTests
{
    private readonly CreateContentDtoValidator _validator;

    public CreateContentDtoValidatorTests()
    {
        _validator = new CreateContentDtoValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Have_Error_When_Title_Is_Empty_Or_Null(string title)
    {
        // Arrange
        var dto = new CreateContentDto(
            title,
            "Valid body content",
            null,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Have_Error_When_Body_Is_Empty_Or_Null(string body)
    {
        // Arrange
        var dto = new CreateContentDto(
            "Valid Title",
            body,
            null,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Body);
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Too_Long()
    {
        // Arrange
        var longTitle = new string('a', 201);
        var dto = new CreateContentDto(
            longTitle,
            "Content body",
            null,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Body_Is_Too_Long()
    {
        // Arrange
        var longBody = new string('a', 10001);
        var dto = new CreateContentDto(
            "Test Title",
            longBody,
            null,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Body);
    }

    [Fact]
    public void Should_Have_Error_When_Summary_Is_Too_Long()
    {
        // Arrange
        var longSummary = new string('a', 501);
        var dto = new CreateContentDto(
            "Test Title",
            "Content body",
            longSummary,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Summary);
    }

    [Fact]
    public void Should_Have_Error_When_AuthorId_Is_Empty()
    {
        // Arrange
        var dto = new CreateContentDto(
            "Test Title",
            "Content body",
            null,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.Empty,
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AuthorId);
    }

    [Fact] 
    public void Should_Have_Error_When_Slug_Exceeds_Maximum_Length()
    {
        // Arrange
        var dto = new CreateContentDto(
            "Test Title",
            "Content body",
            null,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            new string('a', 201), // Exceeds 200 character limit
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Should_Have_Error_When_SortOrder_Is_Negative()
    {
        // Arrange
        var dto = new CreateContentDto(
            "Test Title",
            "Content body",
            null,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            null,
            -1,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.SortOrder);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var dto = new CreateContentDto(
            "Test Title",
            "Content body with sufficient length",
            "Test summary",
            ContentType.Article,
            "image.jpg",
            "Meta Title",
            "Meta Description",
            "tag1,tag2,tag3",
            "Technology",
            Guid.NewGuid(),
            "test-slug",
            1,
            true,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Optional_Fields_Are_Null()
    {
        // Arrange
        var dto = new CreateContentDto(
            "Test Title",
            "Content body",
            null,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("valid-slug")]
    [InlineData("valid-slug-123")]
    [InlineData("123-valid-slug")]
    public void Should_Not_Have_Error_When_Slug_Format_Is_Valid(string slug)
    {
        // Arrange
        var dto = new CreateContentDto(
            "Test Title",
            "Content body",
            null,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            slug,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }

    [Theory]
    [InlineData(ContentType.Article)]
    [InlineData(ContentType.Page)]
    [InlineData(ContentType.BlogPost)]
    public void Should_Not_Have_Error_When_ContentType_Is_Valid(ContentType type)
    {
        // Arrange
        var dto = new CreateContentDto(
            "Test Title",
            "Content body",
            null,
            type,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Summary_Is_Null()
    {
        // Arrange
        var dto = new CreateContentDto(
            "Valid Title",
            "Valid body content",
            null, // Summary is null, which is allowed
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Summary);
    }

    [Fact]
    public void Should_Have_Error_When_MetaTitle_Exceeds_Maximum_Length()
    {
        // Arrange
        var longMetaTitle = new string('a', 101); // Exceeds 100 character limit
        var dto = new CreateContentDto(
            "Valid Title",
            "Valid body content",
            null,
            ContentType.Article,
            null,
            longMetaTitle,
            null,
            null,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.MetaTitle);
    }

    [Fact]
    public void Should_Have_Error_When_MetaDescription_Exceeds_Maximum_Length()
    {
        // Arrange
        var longMetaDescription = new string('a', 301); // Exceeds 300 character limit
        var dto = new CreateContentDto(
            "Valid Title",
            "Valid body content",
            null,
            ContentType.Article,
            null,
            null,
            longMetaDescription,
            null,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.MetaDescription);
    }

    [Fact]
    public void Should_Have_Error_When_Tags_Exceeds_Maximum_Length()
    {
        // Arrange
        var longTags = new string('a', 501); // Exceeds 500 character limit
        var dto = new CreateContentDto(
            "Valid Title",
            "Valid body content",
            null,
            ContentType.Article,
            null,
            null,
            null,
            longTags,
            null,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Should_Have_Error_When_Category_Exceeds_Maximum_Length()
    {
        // Arrange
        var longCategory = new string('a', 101); // Exceeds 100 character limit
        var dto = new CreateContentDto(
            "Valid Title",
            "Valid body content",
            null,
            ContentType.Article,
            null,
            null,
            null,
            null,
            longCategory,
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }
}
