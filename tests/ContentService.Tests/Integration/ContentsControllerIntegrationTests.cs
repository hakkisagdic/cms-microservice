using System.Text;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ContentService.Core.Common;
using ContentService.Core.DTOs;
using ContentService.Core.Entities;
using ContentService.Core.Features.Contents.Commands;
using ContentService.Core.Features.Contents.Queries;
using ContentService.Tests.Helpers;
using Xunit;

namespace ContentService.Tests.Integration;

public class ContentsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IMediator> _mockMediator;
    private readonly HttpClient _client;

    public ContentsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _mockMediator = new Mock<IMediator>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {

                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton(_mockMediator.Object);
            });
        });

        _client = _factory.CreateClient();
        
        // Add JWT token to all requests
        var token = TestJwtTokenHelper.GenerateTestToken();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetAllContents_ShouldReturnOkResult_WhenContentsExist()
    {

        var contents = new List<ContentDto>
        {
            new ContentDto(
                Guid.NewGuid(),
                "Test Content 1",
                "Test Body 1",
                "Test Summary 1",
                ContentStatus.Published,
                ContentType.Article,
                null,
                null,
                null,
                null,
                "Technology",
                100,
                DateTime.UtcNow,
                Guid.NewGuid(),
                "Author 1",
                "test-content-1",
                0,
                false,
                true,
                DateTime.UtcNow,
                null
            ),
            new ContentDto(
                Guid.NewGuid(),
                "Test Content 2",
                "Test Body 2",
                "Test Summary 2",
                ContentStatus.Draft,
                ContentType.Page,
                null,
                null,
                null,
                null,
                "Science",
                50,
                null,
                Guid.NewGuid(),
                "Author 2",
                "test-content-2",
                1,
                true,
                false,
                DateTime.UtcNow,
                DateTime.UtcNow
            )
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetAllContentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<ContentDto>>.Success(contents));

        var response = await _client.GetAsync("/api/contents");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var jsonString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ContentDto[]>(jsonString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.Length.Should().Be(2);
        result[0].Title.Should().Be("Test Content 1");
        result[1].Title.Should().Be("Test Content 2");
    }

    [Fact]
    public async Task GetAllContents_ShouldReturnBadRequest_WhenServiceFails()
    {

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetAllContentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<ContentDto>>.Failure("Service error"));

        var response = await _client.GetAsync("/api/contents");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetContentById_ShouldReturnOkResult_WhenContentExists()
    {

        var contentId = Guid.NewGuid();
        var content = new ContentDto(
            contentId,
            "Test Content",
            "Test Body",
            "Test Summary",
            ContentStatus.Published,
            ContentType.Article,
            null,
            null,
            null,
            null,
            "Technology",
            100,
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Author Name",
            "test-content",
            0,
            false,
            true,
            DateTime.UtcNow,
            null
        );

        _mockMediator
            .Setup(x => x.Send(It.Is<GetContentByIdQuery>(q => q.Id == contentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContentDto?>.Success(content));

        var response = await _client.GetAsync($"/api/contents/{contentId}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var jsonString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ContentDto>(jsonString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.Id.Should().Be(contentId);
        result.Title.Should().Be("Test Content");
    }

    [Fact]
    public async Task GetContentById_ShouldReturnNotFound_WhenContentDoesNotExist()
    {

        var contentId = Guid.NewGuid();

        _mockMediator
            .Setup(x => x.Send(It.Is<GetContentByIdQuery>(q => q.Id == contentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContentDto?>.Failure("Content not found"));

        var response = await _client.GetAsync($"/api/contents/{contentId}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateContent_ShouldReturnCreated_WhenValidRequest()
    {

        var createDto = new CreateContentDto(
            "New Content",
            "New content body",
            "New content summary",
            ContentType.Article,
            null,
            null,
            null,
            null,
            "Technology",
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        var createdContent = new ContentDto(
            Guid.NewGuid(),
            createDto.Title,
            createDto.Body,
            createDto.Summary,
            ContentStatus.Draft,
            createDto.Type,
            createDto.FeaturedImageUrl,
            createDto.MetaTitle,
            createDto.MetaDescription,
            createDto.Tags,
            createDto.Category,
            0,
            null,
            createDto.AuthorId,
            "Author Name",
            "new-content",
            createDto.SortOrder,
            createDto.IsFeatured,
            createDto.AllowComments,
            DateTime.UtcNow,
            null
        );

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateContentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContentDto>.Success(createdContent));

        var json = JsonSerializer.Serialize(createDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/contents", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var jsonString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ContentDto>(jsonString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.Title.Should().Be("New Content");
        result.Status.Should().Be(ContentStatus.Draft);
    }

    [Fact]
    public async Task CreateContent_ShouldReturnBadRequest_WhenCreationFails()
    {

        var createDto = new CreateContentDto(
            "New Content",
            "New content body",
            "New content summary",
            ContentType.Article,
            null,
            null,
            null,
            null,
            "Technology",
            Guid.NewGuid(),
            null,
            0,
            false,
            true
        );

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateContentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContentDto>.Failure("Author not found"));

        var json = JsonSerializer.Serialize(createDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/contents", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateContent_ShouldReturnOk_WhenValidRequest()
    {

        var contentId = Guid.NewGuid();
        var updateDto = new UpdateContentDto(
            "Updated Content",
            "Updated content body",
            "Updated content summary",
            ContentStatus.Draft,
            ContentType.Page,
            null,
            null,
            null,
            null,
            "Science",
            null,
            1,
            true,
            false
        );

        var updatedContent = new ContentDto(
            contentId,
            updateDto.Title,
            updateDto.Body,
            updateDto.Summary,
            ContentStatus.Draft,
            updateDto.Type,
            updateDto.FeaturedImageUrl,
            updateDto.MetaTitle,
            updateDto.MetaDescription,
            updateDto.Tags,
            updateDto.Category,
            0,
            null,
            Guid.NewGuid(),
            "Author Name",
            "updated-content",
            updateDto.SortOrder,
            updateDto.IsFeatured,
            updateDto.AllowComments,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _mockMediator
            .Setup(x => x.Send(It.Is<UpdateContentCommand>(c => c.Id == contentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContentDto>.Success(updatedContent));

        var json = JsonSerializer.Serialize(updateDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/api/contents/{contentId}", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var jsonString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ContentDto>(jsonString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Content");
        result.Type.Should().Be(ContentType.Page);
    }

    [Fact]
    public async Task DeleteContent_ShouldReturnNoContent_WhenContentExists()
    {

        var contentId = Guid.NewGuid();

        _mockMediator
            .Setup(x => x.Send(It.Is<DeleteContentCommand>(c => c.Id == contentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var response = await _client.DeleteAsync($"/api/contents/{contentId}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteContent_ShouldReturnNotFound_WhenContentDoesNotExist()
    {

        var contentId = Guid.NewGuid();

        _mockMediator
            .Setup(x => x.Send(It.Is<DeleteContentCommand>(c => c.Id == contentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Content not found"));

        var response = await _client.DeleteAsync($"/api/contents/{contentId}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PublishContent_ShouldReturnOk_WhenValidRequest()
    {

        var contentId = Guid.NewGuid();
        var publishDto = new PublishContentDto(DateTime.UtcNow.AddHours(1));

        var publishedContent = new ContentDto(
            contentId,
            "Published Content",
            "Published content body",
            "Published content summary",
            ContentStatus.Published,
            ContentType.Article,
            null,
            null,
            null,
            null,
            "Technology",
            0,
            publishDto.PublishAt,
            Guid.NewGuid(),
            "Author Name",
            "published-content",
            0,
            false,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _mockMediator
            .Setup(x => x.Send(It.Is<PublishContentCommand>(c => c.Id == contentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ContentDto>.Success(publishedContent));

        var json = JsonSerializer.Serialize(publishDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"/api/contents/{contentId}/publish", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var jsonString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ContentDto>(jsonString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.Status.Should().Be(ContentStatus.Published);
        result.PublishedAt.Should().NotBeNull();
    }
}
