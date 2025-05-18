using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace vuez.Migrations
{
    /// <inheritdoc />
    public partial class AddVstupnaKontrola : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VstupnaKontrola",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NazovVyrobku = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Dodavatel = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ZakazkoveCislo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KontrolaPodla = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpravnostDodavky = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZnacenieMaterialu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CistotaPovrchu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Balenie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Poskodenie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IneKroky = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Poznamky = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuborCesta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VstupnaKontrola", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "1", 0, "ac2a8d05-4ccf-407f-8c71-e7b8f31a0038", "admin@example.com", true, false, null, "ADMIN@EXAMPLE.COM", "ADMIN", "AQAAAAIAAYagAAAAEM1r6vSjMbtJxGdXCEyqtZwYbbpBRAB4PydiyWffZ6rzRX/edNQVjAscLWToM3CotA==", null, false, "f619d8df-4f3d-4703-98a4-a6a4773d5aaa", false, "admin" });

            migrationBuilder.InsertData(
                table: "Indicators",
                columns: new[] { "Id", "List_Num", "Name", "Producer", "Type" },
                values: new object[,]
                {
                    { 1, "LN001", "Indicator1", "Producer1", "TypeA" },
                    { 2, "LN002", "Indicator2", "Producer2", "TypeB" }
                });

            migrationBuilder.InsertData(
                table: "PdfDocuments",
                columns: new[] { "Id", "FileData", "Name", "UploadedAt" },
                values: new object[] { 1, new byte[0], "Example.pdf", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VstupnaKontrola");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "Indicators",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Indicators",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PdfDocuments",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
