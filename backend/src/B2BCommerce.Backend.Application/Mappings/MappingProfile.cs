using AutoMapper;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping between domain entities and DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.ListPrice, opt => opt.MapFrom(src => src.ListPrice.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.ListPrice.Currency))
            .ForMember(dest => dest.Tier1Price, opt => opt.MapFrom(src => src.Tier1Price != null ? src.Tier1Price.Amount : (decimal?)null))
            .ForMember(dest => dest.Tier2Price, opt => opt.MapFrom(src => src.Tier2Price != null ? src.Tier2Price.Amount : (decimal?)null))
            .ForMember(dest => dest.Tier3Price, opt => opt.MapFrom(src => src.Tier3Price != null ? src.Tier3Price.Amount : (decimal?)null))
            .ForMember(dest => dest.Tier4Price, opt => opt.MapFrom(src => src.Tier4Price != null ? src.Tier4Price.Amount : (decimal?)null))
            .ForMember(dest => dest.Tier5Price, opt => opt.MapFrom(src => src.Tier5Price != null ? src.Tier5Price.Amount : (decimal?)null))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.ProductCategories.FirstOrDefault(pc => pc.IsPrimary) != null ? src.ProductCategories.First(pc => pc.IsPrimary).CategoryId : (Guid?)null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategories.FirstOrDefault(pc => pc.IsPrimary) != null && src.ProductCategories.First(pc => pc.IsPrimary).Category != null ? src.ProductCategories.First(pc => pc.IsPrimary).Category!.Name : null))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt));

        CreateMap<Product, ProductListDto>()
            .ForMember(dest => dest.ListPrice, opt => opt.MapFrom(src => src.ListPrice.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.ListPrice.Currency))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategories.FirstOrDefault(pc => pc.IsPrimary) != null && src.ProductCategories.First(pc => pc.IsPrimary).Category != null ? src.ProductCategories.First(pc => pc.IsPrimary).Category!.Name : null))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null));

        // Customer mappings
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts))
            .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses))
            .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.Attributes));

        // CustomerContact mappings
        CreateMap<CustomerContact, CustomerContactDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()));

        // CustomerAddress mappings
        CreateMap<CustomerAddress, CustomerAddressDto>()
            .ForMember(dest => dest.AddressType, opt => opt.MapFrom(src => src.AddressType.ToString()))
            .ForMember(dest => dest.GeoLocationPathName, opt => opt.MapFrom(src => src.GeoLocation != null ? src.GeoLocation.PathName : null));

        // CustomerAttribute mappings
        CreateMap<CustomerAttribute, CustomerAttributeDto>()
            .ForMember(dest => dest.AttributeCode, opt => opt.MapFrom(src => src.AttributeDefinition != null ? src.AttributeDefinition.Code : string.Empty))
            .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.AttributeDefinition != null ? src.AttributeDefinition.Name : string.Empty));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => src.ApprovalStatus.ToString()))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal.Amount))
            .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.Amount))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount.Amount))
            .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(src => src.ShippingCost.Amount))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Subtotal.Currency))
            .ForMember(dest => dest.CustomerCompanyName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Title : null))
            .ForMember(dest => dest.ShippingStreet, opt => opt.MapFrom(src => src.ShippingAddress.Street))
            .ForMember(dest => dest.ShippingCity, opt => opt.MapFrom(src => src.ShippingAddress.City))
            .ForMember(dest => dest.ShippingState, opt => opt.MapFrom(src => src.ShippingAddress.State))
            .ForMember(dest => dest.ShippingCountry, opt => opt.MapFrom(src => src.ShippingAddress.Country))
            .ForMember(dest => dest.ShippingPostalCode, opt => opt.MapFrom(src => src.ShippingAddress.PostalCode))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt));

        // OrderItem mappings
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.UnitPrice.Currency))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal.Amount))
            .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.Amount))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount.Amount))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount));
    }
}
