using DecorStore.API.Common;
using DecorStore.API.DTOs;

namespace DecorStore.API.Interfaces.Services
{
    public interface ILoadTestingService
    {
        Task<Result<List<string>>> GetAvailableEndpointsAsync();
        Result<LoadTestConfigurationDTO> ValidateConfiguration(LoadTestConfigurationDTO configuration);
        Task<Result<LoadTestResultDTO>> ExecuteLoadTestAsync(LoadTestConfigurationDTO configuration);
    }
}
