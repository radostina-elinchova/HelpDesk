using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addingFavorite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "UsersProjects",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "UsersProjects");
        }
    }
}
