using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Armory.Migrations
{
    /// <inheritdoc />
    public partial class TransactionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "dungeon_transaction_id",
                table: "dungeon_entrances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dungeon_transaction_id",
                table: "dungeon_entrances");
        }
    }
}
