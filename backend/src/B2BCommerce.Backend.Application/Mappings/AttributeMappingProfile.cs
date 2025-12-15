using AutoMapper;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Mappings;

/// <summary>
/// AutoMapper profile for Product Attribute System entities
/// </summary>
public class AttributeMappingProfile : Profile
{
    public AttributeMappingProfile()
    {
        // AttributeDefinition mappings
        CreateMap<AttributeDefinition, AttributeDefinitionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.PredefinedValues, opt => opt.MapFrom(src =>
                src.PredefinedValues.Where(v => !v.IsDeleted).OrderBy(v => v.DisplayOrder)));

        // AttributeValue mappings
        CreateMap<AttributeValue, AttributeValueDto>();

        // ProductType mappings
        CreateMap<ProductType, ProductTypeDto>()
            .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src =>
                src.Attributes.Where(a => !a.IsDeleted).OrderBy(a => a.DisplayOrder)));

        CreateMap<ProductType, ProductTypeListDto>()
            .ForMember(dest => dest.AttributeCount, opt => opt.MapFrom(src =>
                src.Attributes.Count(a => !a.IsDeleted)));

        // ProductTypeAttribute mappings
        CreateMap<ProductTypeAttribute, ProductTypeAttributeDto>()
            .ForMember(dest => dest.AttributeCode, opt => opt.MapFrom(src => src.AttributeDefinition.Code))
            .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.AttributeDefinition.Name))
            .ForMember(dest => dest.AttributeType, opt => opt.MapFrom(src => src.AttributeDefinition.Type.ToString()))
            .ForMember(dest => dest.AttributeExternalId, opt => opt.MapFrom(src => src.AttributeDefinition.ExternalId));

        // ProductAttributeValue mappings
        CreateMap<ProductAttributeValue, ProductAttributeValueDto>()
            .ForMember(dest => dest.AttributeCode, opt => opt.MapFrom(src => src.AttributeDefinition.Code))
            .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.AttributeDefinition.Name))
            .ForMember(dest => dest.AttributeType, opt => opt.MapFrom(src => src.AttributeDefinition.Type.ToString()))
            .ForMember(dest => dest.SelectedValueText, opt => opt.MapFrom(src =>
                src.SelectedValue != null ? src.SelectedValue.DisplayText ?? src.SelectedValue.Value : null));

        // ProductCategory mappings
        CreateMap<ProductCategory, ProductCategoryDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategorySlug, opt => opt.MapFrom(src => src.Category.Slug));
    }
}
