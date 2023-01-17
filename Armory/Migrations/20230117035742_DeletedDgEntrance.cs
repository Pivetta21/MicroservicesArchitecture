using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Armory.Migrations
{
    /// <inheritdoc />
    public partial class DeletedDgEntrance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "deleted",
                table: "dungeon_entrances",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted",
                table: "dungeon_entrances");
        }
    }
}
