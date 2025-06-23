using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using FluentAssertions;
using DecorStore.API.DTOs;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class ImageControllerTests : TestBase
    {
        [Fact]
        public async Task UploadImages_WithValidFiles_ShouldReturnCreated()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var formData = new MultipartFormDataContent();
            
            // Create a test image file
            var imageContent = CreateTestImageContent();
            var fileContent = new ByteArrayContent(imageContent);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            formData.Add(fileContent, "Files", "test-image.jpg");
            formData.Add(new StringContent("test-folder"), "folderName");

            // Act
            var response = await _client.PostAsync("/api/Image/upload", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var uploadResponse = await DeserializeResponseAsync<ImageUploadResponseDTO>(response);
            uploadResponse.Should().NotBeNull();
            uploadResponse!.Images.Should().NotBeEmpty();
            uploadResponse.Images.First().FileName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UploadImages_WithoutFiles_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("test-folder"), "folderName");

            // Act
            var response = await _client.PostAsync("/api/Image/upload", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UploadImages_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var formData = new MultipartFormDataContent();
            var imageContent = CreateTestImageContent();
            var fileContent = new ByteArrayContent(imageContent);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            formData.Add(fileContent, "Files", "test-image.jpg");

            // Act
            var response = await _client.PostAsync("/api/Image/upload", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UploadImages_WithTooManyFiles_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var formData = new MultipartFormDataContent();
            
            // Add 11 files (exceeds limit of 10)
            for (int i = 0; i < 11; i++)
            {
                var imageContent = CreateTestImageContent();
                var fileContent = new ByteArrayContent(imageContent);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                formData.Add(fileContent, "Files", $"test-image-{i}.jpg");
            }
            formData.Add(new StringContent("test-folder"), "folderName");

            // Act
            var response = await _client.PostAsync("/api/Image/upload", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UploadImages_WithInvalidFileType_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var formData = new MultipartFormDataContent();
            
            // Create a text file instead of image
            var textContent = Encoding.UTF8.GetBytes("This is not an image");
            var fileContent = new ByteArrayContent(textContent);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
            formData.Add(fileContent, "Files", "test-file.txt");
            formData.Add(new StringContent("test-folder"), "folderName");

            // Act
            var response = await _client.PostAsync("/api/Image/upload", formData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetImagesByFilePaths_WithValidPaths_ShouldReturnImages()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // First upload an image to get a valid file path
            var uploadFormData = new MultipartFormDataContent();
            var imageContent = CreateTestImageContent();
            var fileContent = new ByteArrayContent(imageContent);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            uploadFormData.Add(fileContent, "Files", "test-image.jpg");
            uploadFormData.Add(new StringContent("test-folder"), "folderName");

            var uploadResponse = await _client.PostAsync("/api/Image/upload", uploadFormData);
            uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var uploadResult = await DeserializeResponseAsync<ImageUploadResponseDTO>(uploadResponse);
            var filePath = uploadResult!.Images.First().FilePath;

            // Act
            var response = await _client.GetAsync($"/api/Image/get-by-filepaths?filePaths={Uri.EscapeDataString(filePath)}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var imageResponse = await DeserializeResponseAsync<ImageUploadResponseDTO>(response);
            imageResponse.Should().NotBeNull();
            imageResponse!.Images.Should().NotBeEmpty();
            imageResponse.Images.First().FilePath.Should().Be(filePath);
        }

        [Fact]
        public async Task GetImagesByFilePaths_WithInvalidPaths_ShouldReturnEmptyResult()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Image/get-by-filepaths?filePaths=invalid/path.jpg");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var imageResponse = await DeserializeResponseAsync<ImageUploadResponseDTO>(response);
            imageResponse.Should().NotBeNull();
            imageResponse!.Images.Should().BeEmpty();
        }

        [Fact]
        public async Task GetImagesByFilePaths_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Image/get-by-filepaths?filePaths=test/path.jpg");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private byte[] CreateTestImageContent()
        {
            // Create a minimal valid JPEG header
            // This is a very basic JPEG file structure for testing
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
