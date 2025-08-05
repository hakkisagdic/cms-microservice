using System.Text.Json;
using ContentService.Core.DTOs;
using ContentService.Core.Interfaces;
using ContentService.Infrastructure.Resilience;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;

namespace ContentService.Infrastructure.Services;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAsyncPolicy<HttpResponseMessage> _resilientPolicy;

    public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _resilientPolicy = PollyPolicies.GetCombinedPolicy();
    }

    private void SetAuthorizationHeader()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(authHeader))
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", authHeader.ToString());
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            SetAuthorizationHeader();

            var response = await _resilientPolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.GetAsync($"api/Users/{userId}", cancellationToken);
            });

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                var user = JsonSerializer.Deserialize<UserDto>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return user;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            _logger.LogWarning("Failed to get user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            SetAuthorizationHeader();

            var response = await _resilientPolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.GetAsync($"api/Users/{userId}", cancellationToken);
            });

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if user {UserId} exists", userId);
            return false;
        }
    }
}
