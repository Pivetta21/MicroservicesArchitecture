using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Migrations
{
    /// <inheritdoc />
    public partial class TransactionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "dungeon_entrances");

            migrationBuilder.AddColumn<Guid>(
                name: "transaction_id",
                table: "dungeons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "transaction_id",
                table: "dungeon_entrances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transaction_id",
                table: "dungeons");

            migrationBuilder.DropColumn(
                name: "transaction_id",
                table: "dungeon_entrances");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "dungeon_entrances",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
