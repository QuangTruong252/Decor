using AutoMapper;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using System.Linq;

namespace DecorStore.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Image mapping
            CreateMap<Image, ImageDTO>();

            // Product mappings
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(img => img.FilePath).ToArray() : new string[0]))
                .ForMember(dest => dest.ImageDetails, opt => opt.MapFrom(src => src.Images));

            CreateMap<CreateProductDTO, Product>()
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

            CreateMap<UpdateProductDTO, Product>()
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

            // Category mappings
            CreateMap<Category, CategoryDTO>()
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : string.Empty))
                .ForMember(dest => dest.Subcategories, opt => opt.MapFrom(src => src.Subcategories));

            CreateMap<CreateCategoryDTO, Category>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategories, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<UpdateCategoryDTO, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategories, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            // Banner mappings
            CreateMap<Banner, BannerDTO>();

            CreateMap<CreateBannerDTO, Banner>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

            CreateMap<UpdateBannerDTO, Banner>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // Review mappings
            CreateMap<Review, ReviewDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    src.User != null ? (src.User.FullName ?? src.User.Username) : "Anonymous"));

            CreateMap<CreateReviewDTO, Review>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            CreateMap<UpdateReviewDTO, Review>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            // Order mappings
            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product != null && src.Product.Images.Any() ? src.Product.Images.First().FilePath : string.Empty))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.UnitPrice * src.Quantity));

            CreateMap<CreateOrderDTO, Order>()
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(_ => "Pending"))
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}
