using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Newsdesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRepos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    License = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Topics = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedOnGitHub = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DiscoveredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    ReadmeExcerpt = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SummaryStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Tldr = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    KeyPoints = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    WhyItMatters = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    SummaryLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PromptVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Repos_DiscoveredOn_Rank",
                table: "Repos",
                columns: new[] { "DiscoveredOn", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_Repos_FullName",
                table: "Repos",
                column: "FullName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Repos");
        }
    }
}
