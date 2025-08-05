using FluentAssertions;
using FluentValidation.TestHelper;
using UserService.Core.DTOs;
using UserService.Core.Validators;
using Xunit;

namespace UserService.Tests.Unit;

public class CreateUserDtoValidatorTests
{
    private readonly CreateUserDtoValidator _validator;

    public CreateUserDtoValidatorTests()
    {
        _validator = new CreateUserDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Empty()
    {

        var dto = new CreateUserDto(
            "",
            "Doe",
            "john@example.com",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Is_Empty()
    {

        var dto = new CreateUserDto(
            "John",
            "",
            "john@example.com",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {

        var dto = new CreateUserDto(
            "John",
            "Doe",
            "invalid-email",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Too_Long()
    {

        var longName = new string('a', 101);
        var dto = new CreateUserDto(
            longName,
            "Doe",
            "john@example.com",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Is_Too_Long()
    {

        var longName = new string('a', 101);
        var dto = new CreateUserDto(
            "John",
            longName,
            "john@example.com",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Too_Long()
    {

        var longEmail = new string('a', 240) + "@example.com";
        var dto = new CreateUserDto(
            "John",
            "Doe",
            longEmail,
            null,
            null,
            null,
            null,
            null,
            null
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Invalid()
    {

        var dto = new CreateUserDto(
            "John",
            "Doe",
            "john@example.com",
            "invalid-phone",
            null,
            null,
            null,
            null,
            null
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Should_Have_Error_When_DateOfBirth_Is_In_Future()
    {

        var futureDate = DateTime.UtcNow.AddDays(1);
        var dto = new CreateUserDto(
            "John",
            "Doe",
            "john@example.com",
            null,
            futureDate,
            null,
            null,
            null,
            null
        );

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {

        var dto = new CreateUserDto(
            "John",
            "Doe",
            "john.doe@example.com",
            "1234567890",
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "profile.jpg",
            "Bio",
            "IT",
            "Developer"
        );

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Optional_Fields_Are_Null()
    {

        var dto = new CreateUserDto(
            "John",
            "Doe",
            "john.doe@example.com",
            null,
            null,
            null,
            null,
            null,
            null
        );

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

        var dto = new CreateUserDto(
            "John",
            "Doe",
            email,
            null,
            null,
            null,
            null,
            null,
            null
        );

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

        var dto = new CreateUserDto(
            "John",
            "Doe",
            "john@example.com",
            phoneNumber,
            null,
            null,
            null,
            null,
            null
        );

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }
}
