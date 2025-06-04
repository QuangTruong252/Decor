using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecorStore.API.Migrations
{
    /// <inheritdoc />
    public partial class EnsureCategoryIdInImagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Categories");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Images",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_CategoryId",
                table: "Images",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Categories_CategoryId",
                table: "Images",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Categories_CategoryId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_CategoryId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Categories",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "");
        }
    }
}
