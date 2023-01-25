using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Armory.Migrations
{
    /// <inheritdoc />
    public partial class RemovePropChar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_playing",
                table: "characters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_playing",
                table: "characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
