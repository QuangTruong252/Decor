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

            // Configure unique indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

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
    }
}
