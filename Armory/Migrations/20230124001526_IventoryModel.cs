using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Armory.Migrations
{
    /// <inheritdoc />
    public partial class IventoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "size",
                table: "inventories");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "inventories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "label",
                table: "inventories",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "inventories");

            migrationBuilder.DropColumn(
                name: "label",
                table: "inventories");

            migrationBuilder.AddColumn<int>(
                name: "size",
                table: "inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
