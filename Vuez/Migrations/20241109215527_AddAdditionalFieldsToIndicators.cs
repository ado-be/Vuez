using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace vuez.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalFieldsToIndicators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Indicators",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Indicators",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Indicators");

            migrationBuilder.AddColumn<string>(
                name: "List_Num",
                table: "Indicators",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Producer",
                table: "Indicators",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Indicators",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "List_Num",
                table: "Indicators");

            migrationBuilder.DropColumn(
                name: "Producer",
                table: "Indicators");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Indicators");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Indicators",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "Indicators",
                columns: new[] { "Id", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Indicator1", 100.0m },
                    { 2, "Indicator2", 200.0m }
                });
        }
    }
}
