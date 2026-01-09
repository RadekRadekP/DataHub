using Grinding.Shared.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grinding.Services
{
    public interface IGrindingService
    {
        Task<IEnumerable<GrindingRestResponseDTO>> GetAllGrindingsAsync();
        Task<GrindingRestResponseDTO?> GetGrindingByIdAsync(int serverId);
        Task<GrindingRestResponseDTO> CreateGrindingAsync(GrindingRestPostRequestDTO grindingDto);
        Task<IEnumerable<GrindingRestResponseDTO>> CreateGrindingsBatchAsync(IEnumerable<GrindingRestPostRequestDTO> grindingDtos);
        // Add Update and Delete method signatures here later if needed
    }
}