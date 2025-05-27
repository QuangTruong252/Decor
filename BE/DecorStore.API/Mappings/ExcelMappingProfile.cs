using AutoMapper;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.Models;

namespace DecorStore.API.Mappings
{
    /// <summary>
    /// AutoMapper profile for Excel import/export DTOs
    /// </summary>
    public class ExcelMappingProfile : Profile
    {
        public ExcelMappingProfile()
        {
            CreateProductMappings();
            CreateCategoryMappings();
            CreateOrderMappings();
            CreateCustomerMappings();
        }

        private void CreateProductMappings()
        {
            // Product to ProductExcelDTO (for export)
            CreateMap<Product, ProductExcelDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => 
                    src.Images != null && src.Images.Any() 
                        ? string.Join(",", src.Images.Select(i => i.FilePath))
                        : string.Empty))
                .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Count : 0))
                .ForMember(dest => dest.TotalSales, opt => opt.MapFrom(src => 
                    src.OrderItems != null ? src.OrderItems.Sum(oi => oi.Quantity) : 0))
                .ForMember(dest => dest.Revenue, opt => opt.MapFrom(src => 
                    src.OrderItems != null ? src.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice) : 0))
                .ForMember(dest => dest.LastSaleDate, opt => opt.MapFrom(src => 
                    src.OrderItems != null && src.OrderItems.Any() 
                        ? src.OrderItems.Max(oi => oi.Order.OrderDate) 
                        : (DateTime?)null))
                .ForMember(dest => dest.ValidationErrors, opt => opt.Ignore())
                .ForMember(dest => dest.RowNumber, opt => opt.Ignore());

            // ProductExcelDTO to Product (for import)
            CreateMap<ProductExcelDTO, Product>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Condition(src => src.CreatedAt.HasValue))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }

        private void CreateCategoryMappings()
        {
            // Category to CategoryExcelDTO (for export)
            CreateMap<Category, CategoryExcelDTO>()
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : string.Empty))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0))
                .ForMember(dest => dest.SubcategoryCount, opt => opt.MapFrom(src => src.Subcategories != null ? src.Subcategories.Count : 0))
                .ForMember(dest => dest.Level, opt => opt.Ignore()) // Calculated separately
                .ForMember(dest => dest.CategoryPath, opt => opt.Ignore()) // Calculated separately
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted))
                .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => 
                    src.Products != null 
                        ? src.Products.SelectMany(p => p.OrderItems ?? new List<OrderItem>()).Sum(oi => oi.Quantity * oi.UnitPrice)
                        : 0))
                .ForMember(dest => dest.AverageProductPrice, opt => opt.MapFrom(src => 
                    src.Products != null && src.Products.Any() 
                        ? src.Products.Average(p => p.Price)
                        : 0))
                .ForMember(dest => dest.ValidationErrors, opt => opt.Ignore())
                .ForMember(dest => dest.RowNumber, opt => opt.Ignore());

            // CategoryExcelDTO to Category (for import)
            CreateMap<CategoryExcelDTO, Category>()
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategories, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Condition(src => src.CreatedAt.HasValue));
        }

        private void CreateOrderMappings()
        {
            // Order to OrderExcelDTO (for export)
            CreateMap<Order, OrderExcelDTO>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.Username}" : string.Empty))
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : string.Empty))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderItems != null ? src.OrderItems.Count : 0))
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => 
                    src.OrderItems != null && src.OrderItems.Any()
                        ? string.Join(",", src.OrderItems.Select(oi => $"{oi.Product.Name}:{oi.Quantity}:{oi.UnitPrice}"))
                        : string.Empty))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => 
                    src.OrderItems != null ? src.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice) : 0))
                .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TotalAmount * 0.1m)) // Assuming 10% tax
                .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(src => 10.00m)) // Flat shipping rate
                .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => 
                    src.OrderItems != null 
                        ? Math.Max(0, src.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice) - src.TotalAmount)
                        : 0))
                .ForMember(dest => dest.DaysSinceOrder, opt => opt.MapFrom(src => (DateTime.UtcNow - src.OrderDate).Days))
                .ForMember(dest => dest.ValidationErrors, opt => opt.Ignore())
                .ForMember(dest => dest.RowNumber, opt => opt.Ignore());

            // OrderExcelDTO to Order (for import)
            CreateMap<OrderExcelDTO, Order>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }

        private void CreateCustomerMappings()
        {
            // Customer to CustomerExcelDTO (for export)
            CreateMap<Customer, CustomerExcelDTO>()
                .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders != null ? src.Orders.Count : 0))
                .ForMember(dest => dest.TotalSpent, opt => opt.MapFrom(src => 
                    src.Orders != null ? src.Orders.Sum(o => o.TotalAmount) : 0))
                .ForMember(dest => dest.AverageOrderValue, opt => opt.MapFrom(src => 
                    src.Orders != null && src.Orders.Any() 
                        ? src.Orders.Average(o => o.TotalAmount)
                        : 0))
                .ForMember(dest => dest.LastOrderDate, opt => opt.MapFrom(src => 
                    src.Orders != null && src.Orders.Any() 
                        ? src.Orders.Max(o => o.OrderDate)
                        : (DateTime?)null))
                .ForMember(dest => dest.DaysSinceLastOrder, opt => opt.MapFrom(src => 
                    src.Orders != null && src.Orders.Any() 
                        ? (int?)(DateTime.UtcNow - src.Orders.Max(o => o.OrderDate)).Days
                        : null))
                .ForMember(dest => dest.LifetimeValue, opt => opt.MapFrom(src => 
                    src.Orders != null ? src.Orders.Sum(o => o.TotalAmount) : 0))
                .ForMember(dest => dest.CustomerStatus, opt => opt.MapFrom(src => 
                    src.Orders != null && src.Orders.Any() && (DateTime.UtcNow - src.Orders.Max(o => o.OrderDate)).Days <= 30 
                        ? "Active" 
                        : src.Orders != null && src.Orders.Any() 
                            ? "Inactive" 
                            : "New"))
                .ForMember(dest => dest.CustomerSegment, opt => opt.MapFrom(src => 
                    src.Orders != null && src.Orders.Count >= 10 && src.Orders.Sum(o => o.TotalAmount) >= 2000 
                        ? "VIP"
                        : src.Orders != null && src.Orders.Count >= 5 && src.Orders.Sum(o => o.TotalAmount) >= 500
                            ? "Loyal"
                            : src.Orders != null && src.Orders.Any()
                                ? "Regular"
                                : "New"))
                .ForMember(dest => dest.ValidationErrors, opt => opt.Ignore())
                .ForMember(dest => dest.RowNumber, opt => opt.Ignore());

            // CustomerExcelDTO to Customer (for import)
            CreateMap<CustomerExcelDTO, Customer>()
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Condition(src => src.CreatedAt.HasValue))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
