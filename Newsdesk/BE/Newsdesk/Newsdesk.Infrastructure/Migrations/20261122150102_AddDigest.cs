using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Newsdesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDigest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleSummaries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ArticleId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tldr = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    KeyPoints = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    WhyItMatters = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PromptVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleSummaries_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DigestEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DigestDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigestEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigestEntries_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleSummaries_ArticleId",
                table: "ArticleSummaries",
                column: "ArticleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DigestEntries_ArticleId",
                table: "DigestEntries",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_DigestEntries_DigestDate_ArticleId",
                table: "DigestEntries",
                columns: new[] { "DigestDate", "ArticleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DigestEntries_DigestDate_Rank",
                table: "DigestEntries",
                columns: new[] { "DigestDate", "Rank" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleSummaries");

            migrationBuilder.DropTable(
                name: "DigestEntries");
        }
    }
}
