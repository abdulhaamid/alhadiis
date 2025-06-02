using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Alhadis.Migrations
{
    /// <inheritdoc />
    public partial class mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Years",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    YearNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Years", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Months",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MonthName = table.Column<string>(type: "text", nullable: false),
                    YearId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Months", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Months_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Weeks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    MonthId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weeks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Weeks_Months_MonthId",
                        column: x => x.MonthId,
                        principalTable: "Months",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hadiths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Content = table.Column<string>(type: "text", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    WeekId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hadiths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hadiths_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hadiths_Weeks_WeekId",
                        column: x => x.WeekId,
                        principalTable: "Weeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Türkçe" },
                    { 2, "Arapça" },
                    { 3, "Oromiçe" },
                    { 4, "Amhariçe" }
                });

            migrationBuilder.InsertData(
                table: "Years",
                columns: new[] { "Id", "YearNumber" },
                values: new object[] { 1, 2025 });

            migrationBuilder.InsertData(
                table: "Months",
                columns: new[] { "Id", "MonthName", "YearId" },
                values: new object[,]
                {
                    { 1, "Ocak", 1 },
                    { 2, "Şubat", 1 },
                    { 3, "Mart", 1 },
                    { 4, "Nisan", 1 },
                    { 5, "Mayıs", 1 },
                    { 6, "Haziran", 1 },
                    { 7, "Temmuz", 1 },
                    { 8, "Ağustos", 1 },
                    { 9, "Eylül", 1 },
                    { 10, "Ekim", 1 },
                    { 11, "Kasım", 1 },
                    { 12, "Aralık", 1 }
                });

            migrationBuilder.InsertData(
                table: "Weeks",
                columns: new[] { "Id", "MonthId", "WeekNumber" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 1, 2 },
                    { 3, 1, 3 },
                    { 4, 1, 4 },
                    { 5, 2, 1 },
                    { 6, 2, 2 },
                    { 7, 2, 3 },
                    { 8, 2, 4 },
                    { 9, 3, 1 },
                    { 10, 3, 2 },
                    { 11, 3, 3 },
                    { 12, 3, 4 },
                    { 13, 4, 1 },
                    { 14, 4, 2 },
                    { 15, 4, 3 },
                    { 16, 4, 4 },
                    { 17, 5, 1 },
                    { 18, 5, 2 },
                    { 19, 5, 3 },
                    { 20, 5, 4 },
                    { 21, 6, 1 },
                    { 22, 6, 2 },
                    { 23, 6, 3 },
                    { 24, 6, 4 },
                    { 25, 7, 1 },
                    { 26, 7, 2 },
                    { 27, 7, 3 },
                    { 28, 7, 4 },
                    { 29, 8, 1 },
                    { 30, 8, 2 },
                    { 31, 8, 3 },
                    { 32, 8, 4 },
                    { 33, 9, 1 },
                    { 34, 9, 2 },
                    { 35, 9, 3 },
                    { 36, 9, 4 },
                    { 37, 10, 1 },
                    { 38, 10, 2 },
                    { 39, 10, 3 },
                    { 40, 10, 4 },
                    { 41, 11, 1 },
                    { 42, 11, 2 },
                    { 43, 11, 3 },
                    { 44, 11, 4 },
                    { 45, 12, 1 },
                    { 46, 12, 2 },
                    { 47, 12, 3 },
                    { 48, 12, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hadiths_LanguageId",
                table: "Hadiths",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Hadiths_WeekId_LanguageId",
                table: "Hadiths",
                columns: new[] { "WeekId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Months_YearId",
                table: "Months",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_Weeks_MonthId",
                table: "Weeks",
                column: "MonthId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hadiths");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Weeks");

            migrationBuilder.DropTable(
                name: "Months");

            migrationBuilder.DropTable(
                name: "Years");
        }
    }
}
