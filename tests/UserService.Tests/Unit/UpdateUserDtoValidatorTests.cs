using FluentAssertions;
using FluentValidation.TestHelper;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Validators;
using Xunit;

namespace UserService.Tests.Unit;

public class UpdateUserDtoValidatorTests
{
    private readonly UpdateUserDtoValidator _validator;

    public UpdateUserDtoValidatorTests()
    {
        _validator = new UpdateUserDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Empty()
    {
        // Arrange
        var dto = new UpdateUserDto(
            "",
            "Doe",
            "john@example.com",
            null,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Is_Empty()
    {
        // Arrange
        var dto = new UpdateUserDto(
            "John",
            "",
            "john@example.com",
            null,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        // Arrange
        var dto = new UpdateUserDto(
            "John",
            "Doe",
            "invalid-email",
            null,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Too_Long()
    {
        // Arrange
        var longName = new string('a', 101);
        var dto = new UpdateUserDto(
            longName,
            "Doe",
            "john@example.com",
            null,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Is_Too_Long()
    {
        // Arrange
        var longName = new string('a', 101);
        var dto = new UpdateUserDto(
            "John",
            longName,
            "john@example.com",
            null,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Too_Long()
    {
                // Arrange - Create an email that exceeds 255 characters
        var longEmail = new string('a', 250) + "@example.com"; // 261 characters total
        var dto = new UpdateUserDto(
            "John",
            "Doe",
            longEmail,
            null,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Invalid()
    {
        // Arrange
        var dto = new UpdateUserDto(
            "John",
            "Doe",
            "john@example.com",
            "invalid-phone",
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Should_Have_Error_When_DateOfBirth_Is_In_Future()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var dto = new UpdateUserDto(
            "John",
            "Doe",
            "john@example.com",
            null,
            futureDate,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var dto = new UpdateUserDto(
            "John",
            "Doe",
            "john.doe@example.com",
            "1234567890",
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UserStatus.Active,
            "profile.jpg",
            "Bio",
            "IT",
            "Developer"
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Optional_Fields_Are_Null()
    {
        // Arrange
        var dto = new UpdateUserDto(
            "John",
            "Doe",
            "john.doe@example.com",
            null,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Status_Is_Different()
    {
        // Arrange
        var dto = new UpdateUserDto(
            "John",
            "Doe",
            "john.doe@example.com",
            null,
            null,
            UserStatus.Inactive,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("john@example.com")]
    [InlineData("john.doe@example.com")]
    [InlineData("john+tag@example.co.uk")]
    [InlineData("user123@domain-name.com")]
    public void Should_Not_Have_Error_When_Email_Format_Is_Valid(string email)
    {
        // Arrange
        var dto = new UpdateUserDto(
            "John",
            "Doe",
            email,
            null,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("+1234567890")]
    [InlineData("(123) 456-7890")]
    [InlineData("123-456-7890")]
    public void Should_Not_Have_Error_When_PhoneNumber_Format_Is_Valid(string phoneNumber)
    {
        // Arrange
        var dto = new UpdateUserDto(
            "John",
            "Doe",
            "john@example.com",
            phoneNumber,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }
}
