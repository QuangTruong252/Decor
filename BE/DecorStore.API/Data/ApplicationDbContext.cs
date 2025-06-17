using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<CategoryImage> CategoryImages { get; set; }
          // Security-related DbSets
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<TokenBlacklist> TokenBlacklists { get; set; }
        public DbSet<SecurityEvent> SecurityEvents { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
        public DbSet<AccountLockout> AccountLockouts { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<ApiKeyUsage> ApiKeyUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global query filter for soft delete
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<OrderItem>().HasQueryFilter(oi => !oi.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Banner>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);

            // Add matching query filter for CartItem to match Product's filter
            modelBuilder.Entity<CartItem>().HasQueryFilter(ci => ci.Product == null || !ci.Product.IsDeleted);

            // Configure unique indexes with filters for soft-delete support
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Slug)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Add performance indexes for frequently queried columns
            // Product performance indexes
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Products_CategoryId");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_Products_IsActive");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.IsFeatured)
                .HasDatabaseName("IX_Products_IsFeatured");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.StockQuantity)
                .HasDatabaseName("IX_Products_StockQuantity");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Price)
                .HasDatabaseName("IX_Products_Price");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Products_CreatedAt");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.AverageRating)
                .HasDatabaseName("IX_Products_AverageRating");

            // Composite indexes for complex queries
            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.CategoryId, p.IsActive, p.IsDeleted })
                .HasDatabaseName("IX_Products_Category_Active_Deleted");

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.IsActive, p.IsFeatured, p.IsDeleted })
                .HasDatabaseName("IX_Products_Active_Featured_Deleted");

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.Price, p.IsActive, p.IsDeleted })
                .HasDatabaseName("IX_Products_Price_Active_Deleted");

            // Order performance indexes
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId)
                .HasDatabaseName("IX_Orders_UserId");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CustomerId)
                .HasDatabaseName("IX_Orders_CustomerId");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderStatus)
                .HasDatabaseName("IX_Orders_OrderStatus");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderDate)
                .HasDatabaseName("IX_Orders_OrderDate");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CreatedAt)
                .HasDatabaseName("IX_Orders_CreatedAt");

            // Composite indexes for dashboard queries
            modelBuilder.Entity<Order>()
                .HasIndex(o => new { o.OrderStatus, o.IsDeleted, o.OrderDate })
                .HasDatabaseName("IX_Orders_Status_Deleted_Date");

            modelBuilder.Entity<Order>()
                .HasIndex(o => new { o.IsDeleted, o.CreatedAt })
                .HasDatabaseName("IX_Orders_Deleted_CreatedAt");

            // OrderItem performance indexes
            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId)
                .HasDatabaseName("IX_OrderItems_OrderId");

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.ProductId)
                .HasDatabaseName("IX_OrderItems_ProductId");

            // Cart performance indexes
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Carts_UserId");

            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.SessionId)
                .HasDatabaseName("IX_Carts_SessionId");

            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.CreatedAt)
                .HasDatabaseName("IX_Carts_CreatedAt");

            // CartItem performance indexes
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.CartId)
                .HasDatabaseName("IX_CartItems_CartId");

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.ProductId)
                .HasDatabaseName("IX_CartItems_ProductId");

            // Review performance indexes
            modelBuilder.Entity<Review>()
                .HasIndex(r => r.ProductId)
                .HasDatabaseName("IX_Reviews_ProductId");

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.UserId)
                .HasDatabaseName("IX_Reviews_UserId");

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.Rating)
                .HasDatabaseName("IX_Reviews_Rating");

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.CreatedAt)
                .HasDatabaseName("IX_Reviews_CreatedAt");

            // Category performance indexes
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.ParentId)
                .HasDatabaseName("IX_Categories_ParentId");

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.IsDeleted)
                .HasDatabaseName("IX_Categories_IsDeleted");

            // User performance indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Role)
                .HasDatabaseName("IX_Users_Role");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.CreatedAt)
                .HasDatabaseName("IX_Users_CreatedAt");

            // Customer performance indexes
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.CreatedAt)
                .HasDatabaseName("IX_Customers_CreatedAt");

            // Security-related performance indexes
            // RefreshToken indexes
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshTokens_Token");

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId)
                .HasDatabaseName("IX_RefreshTokens_UserId");

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.ExpiryDate)
                .HasDatabaseName("IX_RefreshTokens_ExpiryDate");

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenFamily)
                .HasDatabaseName("IX_RefreshTokens_TokenFamily");

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => new { rt.IsUsed, rt.IsRevoked, rt.ExpiryDate })
                .HasDatabaseName("IX_RefreshTokens_Status_Expiry");

            // TokenBlacklist indexes
            modelBuilder.Entity<TokenBlacklist>()
                .HasIndex(tb => tb.JwtId)
                .HasDatabaseName("IX_TokenBlacklist_JwtId");

            modelBuilder.Entity<TokenBlacklist>()
                .HasIndex(tb => tb.TokenHash)
                .HasDatabaseName("IX_TokenBlacklist_TokenHash");

            modelBuilder.Entity<TokenBlacklist>()
                .HasIndex(tb => tb.UserId)
                .HasDatabaseName("IX_TokenBlacklist_UserId");

            modelBuilder.Entity<TokenBlacklist>()
                .HasIndex(tb => tb.ExpiryDate)
                .HasDatabaseName("IX_TokenBlacklist_ExpiryDate");

            modelBuilder.Entity<TokenBlacklist>()
                .HasIndex(tb => tb.BlacklistType)
                .HasDatabaseName("IX_TokenBlacklist_BlacklistType");

            // SecurityEvent indexes
            modelBuilder.Entity<SecurityEvent>()
                .HasIndex(se => se.EventType)
                .HasDatabaseName("IX_SecurityEvents_EventType");

            modelBuilder.Entity<SecurityEvent>()
                .HasIndex(se => se.UserId)
                .HasDatabaseName("IX_SecurityEvents_UserId");

            modelBuilder.Entity<SecurityEvent>()
                .HasIndex(se => se.IpAddress)
                .HasDatabaseName("IX_SecurityEvents_IpAddress");

            modelBuilder.Entity<SecurityEvent>()
                .HasIndex(se => se.Timestamp)
                .HasDatabaseName("IX_SecurityEvents_Timestamp");

            modelBuilder.Entity<SecurityEvent>()
                .HasIndex(se => se.Severity)
                .HasDatabaseName("IX_SecurityEvents_Severity");

            modelBuilder.Entity<SecurityEvent>()
                .HasIndex(se => se.RequiresInvestigation)
                .HasDatabaseName("IX_SecurityEvents_RequiresInvestigation");

            modelBuilder.Entity<SecurityEvent>()
                .HasIndex(se => new { se.EventType, se.Timestamp })
                .HasDatabaseName("IX_SecurityEvents_Type_Timestamp");

            modelBuilder.Entity<SecurityEvent>()
                .HasIndex(se => new { se.UserId, se.Timestamp })
                .HasDatabaseName("IX_SecurityEvents_User_Timestamp");

            modelBuilder.Entity<SecurityEvent>()
                .HasIndex(se => new { se.Success, se.Severity, se.Timestamp })
                .HasDatabaseName("IX_SecurityEvents_Success_Severity_Timestamp");

            // PasswordHistory indexes
            modelBuilder.Entity<PasswordHistory>()
                .HasIndex(ph => ph.UserId)
                .HasDatabaseName("IX_PasswordHistory_UserId");

            modelBuilder.Entity<PasswordHistory>()
                .HasIndex(ph => ph.CreatedAt)
                .HasDatabaseName("IX_PasswordHistory_CreatedAt");

            modelBuilder.Entity<PasswordHistory>()
                .HasIndex(ph => new { ph.UserId, ph.CreatedAt })
                .HasDatabaseName("IX_PasswordHistory_User_CreatedAt");

            // ApiKey indexes
            modelBuilder.Entity<ApiKey>()
                .HasIndex(ak => ak.KeyPrefix)
                .IsUnique()
                .HasDatabaseName("IX_ApiKeys_KeyPrefix");

            modelBuilder.Entity<ApiKey>()
                .HasIndex(ak => ak.CreatedByUserId)
                .HasDatabaseName("IX_ApiKeys_CreatedByUserId");

            modelBuilder.Entity<ApiKey>()
                .HasIndex(ak => ak.ExpiresAt)
                .HasDatabaseName("IX_ApiKeys_ExpiresAt");

            modelBuilder.Entity<ApiKey>()
                .HasIndex(ak => ak.IsActive)
                .HasDatabaseName("IX_ApiKeys_IsActive");

            modelBuilder.Entity<ApiKey>()
                .HasIndex(ak => ak.IsRevoked)
                .HasDatabaseName("IX_ApiKeys_IsRevoked");

            modelBuilder.Entity<ApiKey>()
                .HasIndex(ak => ak.Environment)
                .HasDatabaseName("IX_ApiKeys_Environment");

            modelBuilder.Entity<ApiKey>()
                .HasIndex(ak => ak.CreatedAt)
                .HasDatabaseName("IX_ApiKeys_CreatedAt");

            modelBuilder.Entity<ApiKey>()
                .HasIndex(ak => new { ak.IsActive, ak.IsRevoked, ak.ExpiresAt })
                .HasDatabaseName("IX_ApiKeys_Active_Revoked_Expires");

            // ApiKeyUsage indexes
            modelBuilder.Entity<ApiKeyUsage>()
                .HasIndex(aku => aku.ApiKeyId)
                .HasDatabaseName("IX_ApiKeyUsages_ApiKeyId");

            modelBuilder.Entity<ApiKeyUsage>()
                .HasIndex(aku => aku.CreatedAt)
                .HasDatabaseName("IX_ApiKeyUsages_CreatedAt");

            modelBuilder.Entity<ApiKeyUsage>()
                .HasIndex(aku => aku.IpAddress)
                .HasDatabaseName("IX_ApiKeyUsages_IpAddress");

            modelBuilder.Entity<ApiKeyUsage>()
                .HasIndex(aku => aku.ResponseStatusCode)
                .HasDatabaseName("IX_ApiKeyUsages_ResponseStatusCode");

            modelBuilder.Entity<ApiKeyUsage>()
                .HasIndex(aku => aku.IsSuspicious)
                .HasDatabaseName("IX_ApiKeyUsages_IsSuspicious");

            modelBuilder.Entity<ApiKeyUsage>()
                .HasIndex(aku => new { aku.ApiKeyId, aku.CreatedAt })
                .HasDatabaseName("IX_ApiKeyUsages_ApiKey_CreatedAt");

            modelBuilder.Entity<ApiKeyUsage>()
                .HasIndex(aku => new { aku.IsSuccessful, aku.CreatedAt })
                .HasDatabaseName("IX_ApiKeyUsages_Successful_CreatedAt");

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure many-to-many relationships for Images
            modelBuilder.Entity<ProductImage>()
                .HasKey(pi => new { pi.ProductId, pi.ImageId });

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Image)
                .WithMany(i => i.ProductImages)
                .HasForeignKey(pi => pi.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CategoryImage>()
                .HasKey(ci => new { ci.CategoryId, ci.ImageId });

            modelBuilder.Entity<CategoryImage>()
                .HasOne(ci => ci.Category)
                .WithMany(c => c.CategoryImages)
                .HasForeignKey(ci => ci.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CategoryImage>()
                .HasOne(ci => ci.Image)
                .WithMany(i => i.CategoryImages)
                .HasForeignKey(ci => ci.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure security-related relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.BlacklistedTokens)
                .WithOne(tb => tb.User)
                .HasForeignKey(tb => tb.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.SecurityEvents)
                .WithOne(se => se.User)
                .HasForeignKey(se => se.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .HasMany(u => u.PasswordHistory)
                .WithOne(ph => ph.User)
                .HasForeignKey(ph => ph.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.Subcategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Configure decimal precision
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.OriginalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Cart>()
                .Property(c => c.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // Configure decimal precision for security-related entities
            modelBuilder.Entity<SecurityEvent>()
                .Property(se => se.RiskScore)
                .HasColumnType("decimal(3,2)");

            modelBuilder.Entity<ApiKeyUsage>()
                .Property(aku => aku.RiskScore)
                .HasColumnType("decimal(3,2)");

            // Configure Cart relationships
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Configure CartItem-Product relationship to fix global query filter issue
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Seed data with a fixed date converted to UTC
            var seedDate = new DateTime(2024, 3, 7, 0, 0, 0, DateTimeKind.Utc);

            // Seed sample data (English)
            modelBuilder.Entity<Category>()
                .HasData(
                    new Category
                    {
                        Id = 1,
                        Name = "Lamps",
                        Slug = "lamps",
                        Description = "Decorative Lamps",
                        CreatedAt = seedDate
                    },
                    new Category
                    {
                        Id = 2,
                        Name = "Wall Decor",
                        Slug = "wall-decor",
                        Description = "Wall Decoration Items",
                        CreatedAt = seedDate
                    }
                );

            modelBuilder.Entity<Product>()
                .HasData(
                    new Product
                    {
                        Id = 1,
                        Name = "Decorative Lamp",
                        Slug = "decorative-lamp",
                        Price = 49.99m,
                        CategoryId = 1,
                        SKU = "LAMP001",
                        StockQuantity = 100,
                        CreatedAt = seedDate,
                        UpdatedAt = seedDate
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Wall Clock",
                        Slug = "wall-clock",
                        Price = 35.50m,
                        CategoryId = 2,
                        SKU = "CLOCK001",
                        StockQuantity = 50,
                        CreatedAt = seedDate,
                        UpdatedAt = seedDate
                    }
                );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Suppress pending model changes warning for now
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }
    }
}
