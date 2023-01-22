using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Migrations
{
    /// <inheritdoc />
    public partial class ItemPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "price",
                table: "items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "price",
                table: "items");
        }
    }
}
