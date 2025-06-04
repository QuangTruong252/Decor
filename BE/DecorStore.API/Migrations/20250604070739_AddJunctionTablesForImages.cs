using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecorStore.API.Migrations
{
    /// <inheritdoc />
    public partial class AddJunctionTablesForImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Categories_CategoryId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Products_ProductId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_CategoryId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_ProductId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Images");

            migrationBuilder.CreateTable(
                name: "CategoryImages",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ImageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryImages", x => new { x.CategoryId, x.ImageId });
                    table.ForeignKey(
                        name: "FK_CategoryImages_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ImageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => new { x.ProductId, x.ImageId });
                    table.ForeignKey(
                        name: "FK_ProductImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryImages_ImageId",
                table: "CategoryImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ImageId",
                table: "ProductImages",
                column: "ImageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryImages");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Images",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Images",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_CategoryId",
                table: "Images",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_ProductId",
                table: "Images",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Categories_CategoryId",
                table: "Images",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Products_ProductId",
                table: "Images",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
