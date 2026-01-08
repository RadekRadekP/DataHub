using RPK_BlazorApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPK_BlazorApp.Services
{
    public interface IOperationalService
    {
        Task<IEnumerable<OperationalRestResponseDTO>> GetAllOperationalDataAsync();
        Task<OperationalRestResponseDTO?> GetOperationalDataByIdAsync(int serverId); // Nullable if not found
        Task<OperationalRestResponseDTO> CreateOperationalDataAsync(OperationalRestPostRequestDTO operationalDto);
        Task<IEnumerable<OperationalRestResponseDTO>> CreateOperationalDataBatchAsync(IEnumerable<OperationalRestPostRequestDTO> operationalDtos);
        // We can add Update and Delete methods here later
        // Task<OperationalRestResponseDTO?> UpdateOperationalDataAsync(int id, OperationalRestPostRequestDTO operationalDto);
        // Task<bool> DeleteOperationalDataAsync(int id);
    }
}