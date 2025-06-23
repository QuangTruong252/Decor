using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using FluentAssertions;
using DecorStore.API.DTOs.FileManagement;
using System.Text.Json;
using Xunit;
using System.Net.Http.Json;

namespace DecorStore.Test.Controllers
{
    public class FileManagerControllerTests : TestBase
    {
        [Fact]
        public async Task UploadFiles_WithValidFiles_ShouldReturnSuccess()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var formData = new MultipartFormDataContent();
            
            // Create a test file
            var fileContent = Encoding.UTF8.GetBytes("Test file content");
            var file = new ByteArrayContent(fileContent);
            file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
            formData.Add(file, "files", "test-file.txt");
            
            // Add request parameters
            formData.Add(new StringContent("test-folder"), "FolderPath");
            formData.Add(new StringContent("true"), "CreateThumbnails");
            formData.Add(new StringContent("false"), "OverwriteExisting");

            // Act
            var response = await _client.PostAsync("/api/FileManager/upload", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var uploadResponse = await DeserializeResponseAsync<FileUploadResponseDTO>(response);
            uploadResponse.Should().NotBeNull();
            uploadResponse!.SuccessCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UploadFiles_WithoutFiles_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("test-folder"), "FolderPath");

            // Act
            var response = await _client.PostAsync("/api/FileManager/upload", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UploadFiles_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var formData = new MultipartFormDataContent();
            var fileContent = Encoding.UTF8.GetBytes("Test file content");
            var file = new ByteArrayContent(fileContent);
            formData.Add(file, "files", "test-file.txt");

            // Act
            var response = await _client.PostAsync("/api/FileManager/upload", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateFolder_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var createFolderRequest = new CreateFolderRequestDTO
            {
                ParentPath = "",
                FolderName = "new-test-folder"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/FileManager/create-folder", createFolderRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var folderResponse = await DeserializeResponseAsync<FileItemDTO>(response);
            folderResponse.Should().NotBeNull();
            folderResponse!.Name.Should().Be("new-test-folder");
            folderResponse.Type.Should().Be("folder");
        }

        [Fact]
        public async Task CreateFolder_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var createFolderRequest = new CreateFolderRequestDTO
            {
                ParentPath = "",
                FolderName = "" // Empty folder name
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/FileManager/create-folder", createFolderRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateFolder_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var createFolderRequest = new CreateFolderRequestDTO
            {
                ParentPath = "",
                FolderName = "test-folder"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/FileManager/create-folder", createFolderRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task BrowseFiles_WithValidPath_ShouldReturnContents()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var browseRequest = new FileBrowseRequestDTO
            {
                Path = "",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/FileManager/browse", browseRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var directoryResponse = await DeserializeResponseAsync<FileBrowseResponseDTO>(response);
            directoryResponse.Should().NotBeNull();
            directoryResponse!.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task BrowseFiles_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var browseRequest = new FileBrowseRequestDTO
            {
                Path = "",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/FileManager/browse", browseRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteFiles_WithValidPaths_ShouldReturnSuccess()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // First create a file to delete
            var formData = new MultipartFormDataContent();
            var fileContent = Encoding.UTF8.GetBytes("Test file to delete");
            var file = new ByteArrayContent(fileContent);
            file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
            formData.Add(file, "files", "delete-test.txt");
            formData.Add(new StringContent("test-delete"), "FolderPath");

            var uploadResponse = await _client.PostAsync("/api/FileManager/upload", formData);
            uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var deleteRequest = new DeleteFileRequestDTO
            {
                FilePaths = new List<string> { "test-delete/delete-test.txt" }
            };

            // Act
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/FileManager/delete")
            {
                Content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json")
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var deleteResponse = await DeserializeResponseAsync<DeleteFileResponseDTO>(response);
            deleteResponse.Should().NotBeNull();
            deleteResponse!.SuccessCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DeleteFiles_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var deleteRequest = new DeleteFileRequestDTO
            {
                FilePaths = new List<string> { "test/file.txt" }
            };

            // Act
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/FileManager/delete")
            {
                Content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json")
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GenerateThumbnail_WithValidImagePath_ShouldReturnSuccess()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // First upload an image
            var formData = new MultipartFormDataContent();
            var imageContent = CreateTestImageContent();
            var file = new ByteArrayContent(imageContent);
            file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            formData.Add(file, "files", "test-image.jpg");
            formData.Add(new StringContent("thumbnails"), "FolderPath");

            var uploadResponse = await _client.PostAsync("/api/FileManager/upload", formData);
            uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act
            var response = await _client.PostAsync("/api/FileManager/generate-thumbnail?imagePath=thumbnails/test-image.jpg", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GenerateThumbnail_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.PostAsync("/api/FileManager/generate-thumbnail?imagePath=test/image.jpg", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CleanupOrphanedFiles_ShouldReturnSuccess()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.PostAsync("/api/FileManager/cleanup-orphaned", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var cleanupResponse = await DeserializeResponseAsync<int>(response);
            cleanupResponse.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task CleanupOrphanedFiles_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.PostAsync("/api/FileManager/cleanup-orphaned", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private byte[] CreateTestImageContent()
        {
            // Create a minimal valid JPEG header for testing
            var jpegHeader = new byte[]
            {
                0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
                0x01, 0x01, 0x00, 0x48, 0x00, 0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
                0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
                0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
                0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
                0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
                0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
                0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x00, 0x01,
                0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0x02, 0x11, 0x01, 0x03, 0x11, 0x01,
                0xFF, 0xC4, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0xFF, 0xC4,
                0x00, 0x14, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xDA, 0x00, 0x0C,
                0x03, 0x01, 0x00, 0x02, 0x11, 0x03, 0x11, 0x00, 0x3F, 0x00, 0xB2, 0xC0,
                0x07, 0xFF, 0xD9
            };

            return jpegHeader;
        }
    }
}
