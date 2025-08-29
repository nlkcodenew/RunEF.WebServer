using AutoMapper;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Web.Models;

namespace RunEF.WebServer.Web.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ClientDto, ClientModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.GetHashCode()))
            .ForMember(dest => dest.ComputerCode, opt => opt.MapFrom(src => src.ComputerCode))
            .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
            .ForMember(dest => dest.ComputerName, opt => opt.MapFrom(src => src.ComputerName))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username ?? string.Empty))
            .ForMember(dest => dest.LastHeartbeat, opt => opt.MapFrom(src => src.LastHeartbeat))
            .ForMember(dest => dest.LastSeen, opt => opt.MapFrom(src => src.LastSeen))
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlocked))
            .ForMember(dest => dest.BlockedReason, opt => opt.MapFrom(src => src.BlockedReason))
            .ForMember(dest => dest.BlockedBy, opt => opt.MapFrom(src => (string?)null))
            .ForMember(dest => dest.BlockedAt, opt => opt.MapFrom(src => (DateTime?)null))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => (DateTime?)null));
    }
}