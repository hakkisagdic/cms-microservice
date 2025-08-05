using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using ContentService.Infrastructure.Services;
using ContentService.Core.Interfaces;

namespace ContentService.Tests.Unit;

public class UserServiceClientTests
{
    private readonly Mock<ILogger<UserServiceClient>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly UserServiceClient _userServiceClient;

    public UserServiceClientTests()
    {
        _mockLogger = new Mock<ILogger<UserServiceClient>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        // HttpContext mock'ını yapılandır
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        _userServiceClient = new UserServiceClient(_httpClient, _mockLogger.Object, _mockHttpContextAccessor.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUserDto()
    {

        var userId = Guid.NewGuid();
        var expectedUser = new UserDto(
            userId,
            "John",
            "Doe",
            "test@example.com"
        );

        var jsonResponse = JsonSerializer.Serialize(expectedUser);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage)
            .Verifiable();

        var result = await _userServiceClient.GetUserByIdAsync(userId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedUser.Id);
        result.Email.Should().Be(expectedUser.Email);
        result.FirstName.Should().Be(expectedUser.FirstName);
        result.LastName.Should().Be(expectedUser.LastName);
        
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(), // Polly might retry so allow multiple calls
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserNotFound_ShouldReturnNull()
    {

        var userId = Guid.NewGuid();
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"api/users/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var result = await _userServiceClient.GetUserByIdAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenHttpRequestFails_ShouldLogWarningAndReturnNull()
    {

        var userId = Guid.NewGuid();
        
        // HTTP handler should not return a response at all to simulate failure
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Handler did not return a response message."));

        var result = await _userServiceClient.GetUserByIdAsync(userId);

        result.Should().BeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while getting user {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenExceptionThrown_ShouldLogErrorAndReturnNull()
    {

        var userId = Guid.NewGuid();
        var expectedException = new HttpRequestException("Network error");

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(expectedException);

        var result = await _userServiceClient.GetUserByIdAsync(userId);

        result.Should().BeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while getting user {userId}")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UserExistsAsync_WhenUserExists_ShouldReturnTrue()
    {

        var userId = Guid.NewGuid();
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage)
            .Verifiable();

        var result = await _userServiceClient.UserExistsAsync(userId);

        result.Should().BeTrue();
        
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(), // Polly might retry so allow multiple calls
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task UserExistsAsync_WhenUserNotFound_ShouldReturnFalse()
    {

        var userId = Guid.NewGuid();
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"api/users/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        var result = await _userServiceClient.UserExistsAsync(userId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserExistsAsync_WhenExceptionThrown_ShouldLogErrorAndReturnFalse()
    {

        var userId = Guid.NewGuid();
        var expectedException = new HttpRequestException("Network error");

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(expectedException);

        var result = await _userServiceClient.UserExistsAsync(userId);

        result.Should().BeFalse();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while checking if user {userId} exists")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
