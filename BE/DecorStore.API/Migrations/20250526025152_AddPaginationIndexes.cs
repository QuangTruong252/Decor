using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecorStore.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPaginationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.AlterColumn<string>(
                name: "ShippingState",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingPostalCode",
                table: "Orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCountry",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCity",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "ContactPhone",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Add indexes for pagination and filtering performance

            // Product indexes
            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Price",
                table: "Products",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedAt",
                table: "Products",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UpdatedAt",
                table: "Products",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StockQuantity",
                table: "Products",
                column: "StockQuantity");

            migrationBuilder.CreateIndex(
                name: "IX_Products_AverageRating",
                table: "Products",
                column: "AverageRating");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsFeatured",
                table: "Products",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                table: "Products",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsDeleted",
                table: "Products",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU");

            // Order indexes
            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TotalAmount",
                table: "Orders",
                column: "TotalAmount");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderStatus",
                table: "Orders",
                column: "OrderStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentMethod",
                table: "Orders",
                column: "PaymentMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UpdatedAt",
                table: "Orders",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IsDeleted",
                table: "Orders",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingCity",
                table: "Orders",
                column: "ShippingCity");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingState",
                table: "Orders",
                column: "ShippingState");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingCountry",
                table: "Orders",
                column: "ShippingCountry");

            // Category indexes
            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedAt",
                table: "Categories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsDeleted",
                table: "Categories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

            // Customer indexes
            migrationBuilder.CreateIndex(
                name: "IX_Customers_FirstName",
                table: "Customers",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_LastName",
                table: "Customers",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_City",
                table: "Customers",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_State",
                table: "Customers",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Country",
                table: "Customers",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PostalCode",
                table: "Customers",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedAt",
                table: "Customers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsDeleted",
                table: "Customers",
                column: "IsDeleted");

            // Composite indexes for common query patterns
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_IsActive_IsDeleted",
                table: "Products",
                columns: new[] { "CategoryId", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Price_IsActive_IsDeleted",
                table: "Products",
                columns: new[] { "Price", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_OrderDate",
                table: "Orders",
                columns: new[] { "UserId", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_OrderDate",
                table: "Orders",
                columns: new[] { "CustomerId", "OrderDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_Products_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_UpdatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_StockQuantity",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_AverageRating",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsFeatured",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsDeleted",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SKU",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TotalAmount",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderStatus",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentMethod",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UpdatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_IsDeleted",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingCity",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingState",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingCountry",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CreatedAt",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_IsDeleted",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Customers_FirstName",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_LastName",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_City",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_State",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Country",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PostalCode",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CreatedAt",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IsDeleted",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_IsActive_IsDeleted",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Price_IsActive_IsDeleted",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId_OrderDate",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.AlterColumn<string>(
                name: "ShippingState",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingPostalCode",
                table: "Orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCountry",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCity",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactPhone",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
