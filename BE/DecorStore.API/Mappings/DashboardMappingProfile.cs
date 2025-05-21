using AutoMapper;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using System;
using System.Linq;

namespace DecorStore.API.Mappings
{
    public class DashboardMappingProfile : Profile
    {
        public DashboardMappingProfile()
        {
            // Map Order to RecentOrderDTO
            CreateMap<Order, RecentOrderDTO>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer != null ? src.Customer.FullName :
                    src.User != null ? src.User.Username : "Guest"));

            // Map Product to PopularProductDTO
            CreateMap<Product, PopularProductDTO>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.Images.FirstOrDefault() != null ? src.Images.FirstOrDefault().FilePath : ""));

            // Map Category to CategorySalesDTO
            CreateMap<Category, CategorySalesDTO>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name));
        }
    }
}
