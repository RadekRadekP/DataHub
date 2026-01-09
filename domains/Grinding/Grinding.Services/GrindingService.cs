using Grinding.Shared.Models;
using Grinding.Shared.Dtos;
using GrindingModel = Grinding.Shared.Models.Grinding;
using Grinding.Services.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; 

namespace Grinding.Services
{
    public class GrindingService : IGrindingService
    {
        private readonly GrindingDbContext _context;

        public GrindingService(GrindingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GrindingRestResponseDTO>> GetAllGrindingsAsync()
        {
            return await _context.GrindingData
                .AsNoTracking()
                .Select(g => new GrindingRestResponseDTO
                {
                    Id = g.Id,
                    ClientDbId = g.ClientDbId,
                    ClientId = g.ClientId,
                    ProgramName = g.ProgramName,
                    DateStart = g.DateStart,
                    GrindingTime = g.GrindingTime,
                    FinishTime = g.FinishTime,
                    UpperGWStart = g.UpperGWStart,
                    LowerGWStart = g.LowerGWStart,
                    Operator = g.Operator,
                    Lotto = g.Lotto,
                    GwType = g.GwType,
                    ServerTimestamp = g.ServerTimestamp,
                    ChangeCounter = g.ChangeCounter
                })
                .ToListAsync();
        }

        public async Task<GrindingRestResponseDTO?> GetGrindingByIdAsync(int serverId)
        {
            var grindingEntity = await _context.GrindingData
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == serverId);

            if (grindingEntity == null)
            {
                return null;
            }

            return new GrindingRestResponseDTO
            {
                Id = grindingEntity.Id,
                ClientDbId = grindingEntity.ClientDbId,
                ClientId = grindingEntity.ClientId,
                ProgramName = grindingEntity.ProgramName,
                DateStart = grindingEntity.DateStart,
                GrindingTime = grindingEntity.GrindingTime,
                FinishTime = grindingEntity.FinishTime,
                UpperGWStart = grindingEntity.UpperGWStart,
                LowerGWStart = grindingEntity.LowerGWStart,
                Operator = grindingEntity.Operator,
                Lotto = grindingEntity.Lotto,
                GwType = grindingEntity.GwType,
                ServerTimestamp = grindingEntity.ServerTimestamp,
                ChangeCounter = grindingEntity.ChangeCounter
            };
        }

        public async Task<GrindingRestResponseDTO> CreateGrindingAsync(GrindingRestPostRequestDTO grindingDto)
        {
            var grindingEntity = new GrindingModel // Use Alias
            {
                ClientDbId = grindingDto.ClientDbId,
                ClientId = grindingDto.ClientId,
                ProgramName = grindingDto.ProgramName,
                DateStart = grindingDto.DateStart,
                GrindingTime = TimeSpan.TryParse(grindingDto.GrindingTime, out var gt) ? gt : TimeSpan.Zero, 
                FinishTime = TimeSpan.TryParse(grindingDto.FinishTime, out var ft) ? ft : TimeSpan.Zero, 
                UpperGWStart = grindingDto.UpperGWStart,
                LowerGWStart = grindingDto.LowerGWStart,
                Operator = grindingDto.Operator,
                Lotto = grindingDto.Lotto,
                GwType = grindingDto.GwType
            };

            _context.GrindingData.Add(grindingEntity);
            await _context.SaveChangesAsync();

            return new GrindingRestResponseDTO
            {
                Id = grindingEntity.Id,
                ClientDbId = grindingEntity.ClientDbId,
                ClientId = grindingEntity.ClientId,
                ProgramName = grindingEntity.ProgramName,
                DateStart = grindingEntity.DateStart,
                GrindingTime = grindingEntity.GrindingTime,
                FinishTime = grindingEntity.FinishTime,
                UpperGWStart = grindingEntity.UpperGWStart,
                LowerGWStart = grindingEntity.LowerGWStart,
                Operator = grindingEntity.Operator,
                Lotto = grindingEntity.Lotto,
                GwType = grindingEntity.GwType,
                ServerTimestamp = grindingEntity.ServerTimestamp, 
                ChangeCounter = grindingEntity.ChangeCounter   
            };
        }

        public async Task<IEnumerable<GrindingRestResponseDTO>> CreateGrindingsBatchAsync(IEnumerable<GrindingRestPostRequestDTO> grindingDtos)
        {
            var newGrindingEntities = new List<GrindingModel>(); // Use Alias

            foreach (var dto in grindingDtos)
            {
                var grindingEntity = new GrindingModel // Use Alias
                {
                    ClientDbId = dto.ClientDbId,
                    ClientId = dto.ClientId,
                    ProgramName = dto.ProgramName,
                    DateStart = dto.DateStart,
                    GrindingTime = TimeSpan.TryParse(dto.GrindingTime, out var gt) ? gt : TimeSpan.Zero,
                    FinishTime = TimeSpan.TryParse(dto.FinishTime, out var ft) ? ft : TimeSpan.Zero,
                    UpperGWStart = dto.UpperGWStart,
                    LowerGWStart = dto.LowerGWStart,
                    Operator = dto.Operator,
                    Lotto = dto.Lotto,
                    GwType = dto.GwType
                };
                newGrindingEntities.Add(grindingEntity);
            }

            if (!newGrindingEntities.Any())
            {
                return Enumerable.Empty<GrindingRestResponseDTO>();
            }

            await _context.GrindingData.AddRangeAsync(newGrindingEntities);
            await _context.SaveChangesAsync();

            return newGrindingEntities.Select(entity => new GrindingRestResponseDTO
            {
                Id = entity.Id,
                ClientDbId = entity.ClientDbId,
                ClientId = entity.ClientId,
                ProgramName = entity.ProgramName,
                DateStart = entity.DateStart,
                GrindingTime = entity.GrindingTime,
                FinishTime = entity.FinishTime,
                UpperGWStart = entity.UpperGWStart,
                LowerGWStart = entity.LowerGWStart,
                Operator = entity.Operator,
                Lotto = entity.Lotto,
                GwType = entity.GwType,
                ServerTimestamp = entity.ServerTimestamp,
                ChangeCounter = entity.ChangeCounter
            }).ToList();
        }
    }
}