using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecorStore.API.Migrations
{
    /// <summary>
    /// Phase 5: Performance & Caching - Critical Database Indexes
    /// Adds high-performance indexes for frequently queried columns
    /// </summary>
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Products Performance Indexes
            
            // Index for category-based product filtering (most common query pattern)
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_IsActive_CreatedDate",
                table: "Products",
                columns: new[] { "CategoryId", "IsActive", "CreatedDate" });
            
            // Unique index for SKU lookups (business requirement)
            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU_Unique",
                table: "Products",
                column: "SKU",
                unique: true,
                filter: "[IsDeleted] = 0");
            
            // Unique index for product slug lookups (SEO URLs)
            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug_Unique", 
                table: "Products",
                column: "Slug",
                unique: true,
                filter: "[IsDeleted] = 0");
            
            // Index for price range filtering
            migrationBuilder.CreateIndex(
                name: "IX_Products_Price_IsActive",
                table: "Products",
                columns: new[] { "Price", "IsActive" });
            
            // Index for stock quantity filtering (low stock alerts)
            migrationBuilder.CreateIndex(
                name: "IX_Products_StockQuantity_IsActive",
                table: "Products",
                columns: new[] { "StockQuantity", "IsActive" });
            
            // Index for featured products
            migrationBuilder.CreateIndex(
                name: "IX_Products_IsFeatured_IsActive_CreatedDate",
                table: "Products",
                columns: new[] { "IsFeatured", "IsActive", "CreatedDate" });
            
            // Index for product rating queries
            migrationBuilder.CreateIndex(
                name: "IX_Products_AverageRating_IsActive",
                table: "Products",
                columns: new[] { "AverageRating", "IsActive" });

            // Orders Performance Indexes
            
            // Index for customer order history (most frequent query)
            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_OrderDate",
                table: "Orders",
                columns: new[] { "CustomerId", "OrderDate" });
            
            // Index for user order history
            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_OrderDate", 
                table: "Orders",
                columns: new[] { "UserId", "OrderDate" });
            
            // Index for order status filtering
            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_CreatedDate",
                table: "Orders",
                columns: new[] { "OrderStatus", "CreatedDate" });
            
            // Index for order total amount queries (analytics)
            migrationBuilder.CreateIndex(
                name: "IX_Orders_TotalAmount_OrderDate",
                table: "Orders",
                columns: new[] { "TotalAmount", "OrderDate" });

            // CartItems Performance Indexes
            
            // Index for cart lookups (session-based shopping)
            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId_ProductId",
                table: "CartItems",
                columns: new[] { "UserId", "ProductId" });
            
            // Index for cart session lookups
            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId" });

            // Categories Performance Indexes
            
            // Index for category hierarchy navigation
            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId_SortOrder",
                table: "Categories",
                columns: new[] { "ParentId", "SortOrder" });
            
            // Unique index for category slug lookups
            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug_Unique",
                table: "Categories", 
                column: "Slug",
                unique: true,
                filter: "[IsDeleted] = 0");

            // Customers Performance Indexes
            
            // Unique index for customer email lookups (authentication)
            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email_Unique",
                table: "Customers",
                column: "Email",
                unique: true,
                filter: "[IsDeleted] = 0");
            
            // Index for customer creation date (analytics)
            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedAt",
                table: "Customers",
                column: "CreatedAt");

            // OrderItems Performance Indexes
            
            // Index for order item product lookups
            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId_OrderId",
                table: "OrderItems",
                columns: new[] { "ProductId", "OrderId" });

            // Images Performance Indexes
            
            // Index for image file path lookups
            migrationBuilder.CreateIndex(
                name: "IX_Images_FilePath",
                table: "Images",
                column: "FilePath");
            
            // Index for image folder queries
            migrationBuilder.CreateIndex(
                name: "IX_Images_FolderName_CreatedAt",
                table: "Images",
                columns: new[] { "FolderName", "CreatedAt" });

            // Junction Table Indexes (for many-to-many relationships)
            
            // ProductImages junction table optimization
            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_ImageId",
                table: "ProductImages",
                columns: new[] { "ProductId", "ImageId" });
            
            // CategoryImages junction table optimization
            migrationBuilder.CreateIndex(
                name: "IX_CategoryImages_CategoryId_ImageId", 
                table: "CategoryImages",
                columns: new[] { "CategoryId", "ImageId" });

            // Reviews Performance Indexes
            
            // Index for product reviews
            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId_CreatedAt",
                table: "Reviews",
                columns: new[] { "ProductId", "CreatedAt" });
            
            // Index for customer reviews
            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CustomerId_CreatedAt",
                table: "Reviews",
                columns: new[] { "CustomerId", "CreatedAt" });

            // Users Performance Indexes
            
            // Unique index for user email lookups
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_Unique",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[IsDeleted] = 0");
            
            // Unique index for username lookups
            migrationBuilder.CreateIndex(
                name: "IX_Users_Username_Unique",
                table: "Users",
                column: "Username", 
                unique: true,
                filter: "[IsDeleted] = 0");

            // Carts Performance Indexes
            
            // Index for user cart lookups
            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId");
            
            // Index for session cart lookups
            migrationBuilder.CreateIndex(
                name: "IX_Carts_SessionId",
                table: "Carts",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all performance indexes in reverse order
            
            // Carts
            migrationBuilder.DropIndex(name: "IX_Carts_SessionId", table: "Carts");
            migrationBuilder.DropIndex(name: "IX_Carts_UserId", table: "Carts");
            
            // Users
            migrationBuilder.DropIndex(name: "IX_Users_Username_Unique", table: "Users");
            migrationBuilder.DropIndex(name: "IX_Users_Email_Unique", table: "Users");
            
            // Reviews
            migrationBuilder.DropIndex(name: "IX_Reviews_CustomerId_CreatedAt", table: "Reviews");
            migrationBuilder.DropIndex(name: "IX_Reviews_ProductId_CreatedAt", table: "Reviews");
            
            // Junction Tables
            migrationBuilder.DropIndex(name: "IX_CategoryImages_CategoryId_ImageId", table: "CategoryImages");
            migrationBuilder.DropIndex(name: "IX_ProductImages_ProductId_ImageId", table: "ProductImages");
            
            // Images
            migrationBuilder.DropIndex(name: "IX_Images_FolderName_CreatedAt", table: "Images");
            migrationBuilder.DropIndex(name: "IX_Images_FilePath", table: "Images");
            
            // OrderItems
            migrationBuilder.DropIndex(name: "IX_OrderItems_ProductId_OrderId", table: "OrderItems");
            
            // Customers
            migrationBuilder.DropIndex(name: "IX_Customers_CreatedAt", table: "Customers");
            migrationBuilder.DropIndex(name: "IX_Customers_Email_Unique", table: "Customers");
            
            // Categories
            migrationBuilder.DropIndex(name: "IX_Categories_Slug_Unique", table: "Categories");
            migrationBuilder.DropIndex(name: "IX_Categories_ParentCategoryId_SortOrder", table: "Categories");
            
            // CartItems
            migrationBuilder.DropIndex(name: "IX_CartItems_CartId_ProductId", table: "CartItems");
            migrationBuilder.DropIndex(name: "IX_CartItems_UserId_ProductId", table: "CartItems");
            
            // Orders
            migrationBuilder.DropIndex(name: "IX_Orders_TotalAmount_OrderDate", table: "Orders");
            migrationBuilder.DropIndex(name: "IX_Orders_Status_CreatedDate", table: "Orders");
            migrationBuilder.DropIndex(name: "IX_Orders_UserId_OrderDate", table: "Orders");
            migrationBuilder.DropIndex(name: "IX_Orders_CustomerId_OrderDate", table: "Orders");
            
            // Products
            migrationBuilder.DropIndex(name: "IX_Products_AverageRating_IsActive", table: "Products");
            migrationBuilder.DropIndex(name: "IX_Products_IsFeatured_IsActive_CreatedDate", table: "Products");
            migrationBuilder.DropIndex(name: "IX_Products_StockQuantity_IsActive", table: "Products");
            migrationBuilder.DropIndex(name: "IX_Products_Price_IsActive", table: "Products");
            migrationBuilder.DropIndex(name: "IX_Products_Slug_Unique", table: "Products");
            migrationBuilder.DropIndex(name: "IX_Products_SKU_Unique", table: "Products");
            migrationBuilder.DropIndex(name: "IX_Products_CategoryId_IsActive_CreatedDate", table: "Products");
        }
    }
}
