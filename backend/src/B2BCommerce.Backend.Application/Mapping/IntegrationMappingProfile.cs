using AutoMapper;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Domain.Entities.Integration;

namespace B2BCommerce.Backend.Application.Mapping;

/// <summary>
/// AutoMapper profile for Integration API entities
/// </summary>
public class IntegrationMappingProfile : Profile
{
    public IntegrationMappingProfile()
    {
        // ApiClient mappings
        CreateMap<ApiClient, ApiClientListDto>()
            .ForMember(d => d.ActiveKeyCount,
                o => o.MapFrom(s => s.ApiKeys.Count(k => k.IsActive && !k.IsExpired())));

        CreateMap<ApiClient, ApiClientDetailDto>()
            .ForMember(d => d.ApiKeys, o => o.MapFrom(s => s.ApiKeys));

        // ApiKey mappings
        CreateMap<ApiKey, ApiKeyListDto>()
            .ForMember(d => d.IsExpired, o => o.MapFrom(s => s.IsExpired()))
            .ForMember(d => d.IsRevoked, o => o.MapFrom(s => s.IsRevoked()))
            .ForMember(d => d.PermissionCount, o => o.MapFrom(s => s.Permissions.Count));

        CreateMap<ApiKey, ApiKeyDetailDto>()
            .ForMember(d => d.ApiClientName, o => o.MapFrom(s => s.ApiClient.Name))
            .ForMember(d => d.IsExpired, o => o.MapFrom(s => s.IsExpired()))
            .ForMember(d => d.IsRevoked, o => o.MapFrom(s => s.IsRevoked()))
            .ForMember(d => d.Permissions, o => o.MapFrom(s => s.Permissions.Select(p => p.Scope)))
            .ForMember(d => d.IpWhitelist, o => o.MapFrom(s => s.IpWhitelist));

        // IP Whitelist mapping
        CreateMap<ApiKeyIpWhitelist, IpWhitelistDto>();

        // Usage Log mapping
        CreateMap<ApiKeyUsageLog, ApiKeyUsageLogDto>();
    }
}
