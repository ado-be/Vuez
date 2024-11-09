using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace vuez.Migrations
{
    /// <inheritdoc />
    public partial class AddIndicatorsSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Indicators",
                columns: new[] { "Id", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Indicator1", 100.0m },
                    { 2, "Indicator2", 200.0m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Indicators",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Indicators",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
