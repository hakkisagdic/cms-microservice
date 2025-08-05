using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Tests.Helpers;
using Xunit;

namespace UserService.Tests.Integration;

public class UsersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UsersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        
        // Add JWT token to all requests
        var token = TestJwtTokenHelper.GenerateTestToken();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOk_WhenCalled()
    {

        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnCreated_WhenValidUserData()
    {

        var createUserDto = new CreateUserDto(
            "Test",
            "User",
            "test@example.com",
            "1234567890",
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "profile.jpg",
            "Test bio",
            "IT",
            "Developer"
        );

        var response = await _client.PostAsJsonAsync("/api/users", createUserDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto!.FirstName.Should().Be(createUserDto.FirstName);
        userDto.LastName.Should().Be(createUserDto.LastName);
        userDto.Email.Should().Be(createUserDto.Email);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenInvalidData()
    {

        var invalidDto = new CreateUserDto(
            "",
            "User",
            "invalid-email",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var response = await _client.PostAsJsonAsync("/api/users", invalidDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {

        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/users/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {

        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpdateUserDto(
            "Updated",
            "User",
            "updated@example.com",
            null,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        var response = await _client.PutAsJsonAsync($"/api/users/{nonExistentId}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {

        var nonExistentId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/users/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnConflict_WhenEmailAlreadyExists()
    {

        var email = "duplicate@example.com";
        var firstUser = new CreateUserDto(
            "First",
            "User",
            email,
            null,
            null,
            null,
            null,
            null,
            null
        );

        var secondUser = new CreateUserDto(
            "Second",
            "User",
            email,
            null,
            null,
            null,
            null,
            null,
            null
        );

        var firstResponse = await _client.PostAsJsonAsync("/api/users", firstUser);
        var secondResponse = await _client.PostAsJsonAsync("/api/users", secondUser);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UserWorkflow_ShouldWork_WhenFullCrudOperations()
    {

        var createDto = new CreateUserDto(
            "Workflow",
            "User",
            "workflow@example.com",
            "1234567890",
            new DateTime(1985, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            "workflow.jpg",
            "Workflow test user",
            "QA",
            "Tester"
        );

        var createResponse = await _client.PostAsJsonAsync("/api/users", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();
        createdUser.Should().NotBeNull();
        var userId = createdUser!.Id;

        var getResponse = await _client.GetAsync($"/api/users/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retrievedUser = await getResponse.Content.ReadFromJsonAsync<UserDto>();
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Id.Should().Be(userId);
        retrievedUser.FirstName.Should().Be(createDto.FirstName);

        var updateDto = new UpdateUserDto(
            "Updated Workflow",
            "Updated User",
            "updated.workflow@example.com",
            "0987654321",
            new DateTime(1986, 6, 16, 0, 0, 0, DateTimeKind.Utc),
            UserStatus.Active,
            "updated.jpg",
            "Updated workflow test user",
            "Development",
            "Senior Tester"
        );

        var updateResponse = await _client.PutAsJsonAsync($"/api/users/{userId}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await updateResponse.Content.ReadFromJsonAsync<UserDto>();
        updatedUser.Should().NotBeNull();
        updatedUser!.FirstName.Should().Be(updateDto.FirstName);
        updatedUser.LastName.Should().Be(updateDto.LastName);

        var deleteResponse = await _client.DeleteAsync($"/api/users/{userId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/users/{userId}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
