using AutoMapper;
using Style365.Application.Common.DTOs;
using Style365.Domain.Entities;

namespace Style365.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value));

        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory!.Name))
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency))
            .ForMember(dest => dest.ComparePrice, opt => opt.MapFrom(src => src.ComparePrice!.Amount))
            .ForMember(dest => dest.CostPrice, opt => opt.MapFrom(src => src.CostPrice!.Amount))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => t.Name)));

        CreateMap<ProductImage, ProductImageDto>();

        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price!.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price!.Currency))
            .ForMember(dest => dest.ComparePrice, opt => opt.MapFrom(src => src.ComparePrice!.Amount));
    }
}