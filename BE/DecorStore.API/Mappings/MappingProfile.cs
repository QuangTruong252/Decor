using AutoMapper;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using System.Linq;

namespace DecorStore.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {            // Image mapping
            CreateMap<Image, ImageDTO>();
            CreateMap<Image, ImageResponseDTO>();

            // User mappings
            CreateMap<User, UserDTO>();
            CreateMap<RegisterDTO, User>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore())
                .ForMember(dest => dest.BlacklistedTokens, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityEvents, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHistory, opt => opt.Ignore());

            // Product mappings
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => 
                    src.ProductImages != null ? src.ProductImages.Select(pi => pi.Image.FilePath).ToArray() : new string[0]))
                .ForMember(dest => dest.ImageDetails, opt => opt.MapFrom(src => 
                    src.ProductImages != null ? src.ProductImages.Select(pi => pi.Image) : Enumerable.Empty<Image>()));

            CreateMap<CreateProductDTO, Product>()
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

            CreateMap<UpdateProductDTO, Product>()
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

            // Category mappings
            CreateMap<Category, CategoryDTO>()
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : string.Empty))
                .ForMember(dest => dest.Subcategories, opt => opt.MapFrom(src => src.Subcategories))
                .ForMember(dest => dest.ImageDetails, opt => opt.MapFrom(src => 
                    src.CategoryImages != null ? src.CategoryImages.Select(ci => ci.Image) : Enumerable.Empty<Image>()));

            CreateMap<CreateCategoryDTO, Category>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.CategoryImages, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategories, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<UpdateCategoryDTO, Category>()
                .ForMember(dest => dest.CategoryImages, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategories, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());            // Banner mappings
            CreateMap<Banner, BannerDTO>()
                .ForMember(dest => dest.LinkUrl, opt => opt.MapFrom(src => src.Link));

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
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.FullName : "Unknown User"))
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src =>
                    src.Customer != null ? $"{src.Customer.FirstName} {src.Customer.LastName}".Trim() : "Guest Customer"))
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => 
                    src.Product != null && src.Product.ProductImages.Any() ? 
                    src.Product.ProductImages.First().Image.FilePath : string.Empty))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.UnitPrice * src.Quantity));

            CreateMap<CreateOrderDTO, Order>()
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(_ => "Pending"))
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore());

            CreateMap<UpdateOrderDTO, Order>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.OrderStatus, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore());

            // Cart mappings
            CreateMap<Cart, CartDTO>()
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity)))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<CartItem, CartItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductSlug, opt => opt.MapFrom(src => src.Product.Slug))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src =>
                    src.Product.ProductImages.Any() ? src.Product.ProductImages.First().Image.FilePath : null))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));

            // Customer mappings
            CreateMap<Customer, CustomerDTO>();

            CreateMap<CreateCustomerDTO, Customer>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.Orders, opt => opt.Ignore());

            CreateMap<UpdateCustomerDTO, Customer>()
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => System.DateTime.UtcNow))
                .ForMember(dest => dest.Orders, opt => opt.Ignore());
        }
    }
}
