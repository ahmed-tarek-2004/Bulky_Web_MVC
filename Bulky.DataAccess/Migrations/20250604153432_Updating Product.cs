using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "Title",
                value: "Fortune of Time");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "Title",
                value: "Dark Skies");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "Title",
                value: "Vanish in the Sunset");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "Title",
                value: "Cotton Candy");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "Title",
                value: "Rock in the Ocean");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "Title",
                value: "Leaves and Wonders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Products");
        }
    }
}
