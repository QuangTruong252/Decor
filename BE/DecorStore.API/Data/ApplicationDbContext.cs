using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DecorStore.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure index for User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
                
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
                
            // Configure decimal precision for Price (PostgreSQL)
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
            
            // Use identity columns in PostgreSQL
            modelBuilder.UseIdentityByDefaultColumns();
                
            // Seed data with a fixed date converted to UTC
            var seedDate = new DateTime(2024, 3, 7, 0, 0, 0, DateTimeKind.Utc);
                
            // Seed sample data (English)
            modelBuilder.Entity<Product>()
                .HasData(
                    new Product 
                    { 
                        Id = 1, 
                        Name = "Decorative Lamp", 
                        Price = 49.99m, 
                        Category = "Lamps",
                        ImageUrl = "/images/products/lamp.jpg",
                        CreatedAt = seedDate,
                        UpdatedAt = seedDate
                    },
                    new Product 
                    { 
                        Id = 2, 
                        Name = "Wall Clock", 
                        Price = 35.50m, 
                        Category = "Wall Decor",
                        ImageUrl = "/images/products/clock.jpg",
                        CreatedAt = seedDate,
                        UpdatedAt = seedDate
                    },
                    new Product 
                    { 
                        Id = 3, 
                        Name = "Cushion Set", 
                        Price = 22.99m, 
                        Category = "Fabric",
                        ImageUrl = "/images/products/cushion.jpg",
                        CreatedAt = seedDate,
                        UpdatedAt = seedDate
                    }
                );
        }
    }
} 