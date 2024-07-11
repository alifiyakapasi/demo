using AutoMapper;
using DemoApiMongo.Entities.DataModels;
using DemoApiMongo.Entities.ViewModels;

namespace DemoApiMongo.Entities.Mappers
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<ProductDetailModel, ProductDetails>()
                 .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                 .ReverseMap().ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName));

            CreateMap<ProductCategoryModel, CategoryList>()
                 .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                 .ReverseMap().ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName));
        }

    }
}
