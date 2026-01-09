using AutoMapper;
using Grinding.Shared.Models;
using Grinding.Shared.Dtos;
using DataHub.Core.Models.UI;
using Eisod.Shared.Models;

namespace DataHub.Host.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Alarm Mappings
            CreateMap<Alarm, AlarmUIModel>();
            CreateMap<AlarmUIModel, Alarm>();
            CreateMap<Alarm, AlarmRestResponseDTO>();
            CreateMap<AlarmRestPostRequestDTO, Alarm>();

            // Operational Mappings
            CreateMap<Operational, OperationalRestResponseDTO>();
            CreateMap<OperationalRestPostRequestDTO, Operational>();

            // Grinding Mappings
            CreateMap<Grinding.Shared.Models.Grinding, GrindingRestResponseDTO>();
            CreateMap<GrindingRestPostRequestDTO, Grinding.Shared.Models.Grinding>();

            // ViewEisodSd Mappings
            CreateMap<ViewEisodSd, ViewEisodSdUIModel>();
            CreateMap<ViewEisodSdUIModel, ViewEisodSd>();
        }
    }
}
