using RPK_BlazorApp.Models;
using RPK_BlazorApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // For TimeSpan.Parse

namespace RPK_BlazorApp.Services
{
    public class GrindingService : IGrindingService
    {
        private readonly ApplicationDbContext _context;

        public GrindingService(ApplicationDbContext context)
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
            var grindingEntity = new Grinding
            {
                ClientDbId = grindingDto.ClientDbId,
                ClientId = grindingDto.ClientId,
                ProgramName = grindingDto.ProgramName,
                DateStart = grindingDto.DateStart,
                GrindingTime = TimeSpan.TryParse(grindingDto.GrindingTime, out var gt) ? gt : TimeSpan.Zero, // Handle potential parse error
                FinishTime = TimeSpan.TryParse(grindingDto.FinishTime, out var ft) ? ft : TimeSpan.Zero, // Handle potential parse error
                UpperGWStart = grindingDto.UpperGWStart,
                LowerGWStart = grindingDto.LowerGWStart,
                Operator = grindingDto.Operator,
                Lotto = grindingDto.Lotto,
                GwType = grindingDto.GwType
            };

            _context.GrindingData.Add(grindingEntity);
            await _context.SaveChangesAsync();

            // Map directly to avoid potential null and extra DB call
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
                ServerTimestamp = grindingEntity.ServerTimestamp, // Will reflect DB generated value
                ChangeCounter = grindingEntity.ChangeCounter   // Will reflect DB generated value
            };
        }

        public async Task<IEnumerable<GrindingRestResponseDTO>> CreateGrindingsBatchAsync(IEnumerable<GrindingRestPostRequestDTO> grindingDtos)
        {
            var newGrindingEntities = new List<Grinding>();

            foreach (var dto in grindingDtos)
            {
                var grindingEntity = new Grinding
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
                    // ServerTimestamp and ChangeCounter will use their default values
                    // Id will be auto-generated by the database
                };
                newGrindingEntities.Add(grindingEntity);
            }

            if (!newGrindingEntities.Any())
            {
                return Enumerable.Empty<GrindingRestResponseDTO>();
            }

            await _context.GrindingData.AddRangeAsync(newGrindingEntities);
            await _context.SaveChangesAsync();

            // Map the newly created entities (now with server-generated IDs) back to ResponseDTOs
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