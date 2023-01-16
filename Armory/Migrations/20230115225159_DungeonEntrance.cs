using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Armory.Migrations
{
    /// <inheritdoc />
    public partial class DungeonEntrance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dungeon_entrances",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transactionid = table.Column<Guid>(name: "transaction_id", type: "uuid", nullable: false),
                    payedfee = table.Column<long>(name: "payed_fee", type: "bigint", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    characterid = table.Column<long>(name: "character_id", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dungeon_entrances", x => x.id);
                    table.ForeignKey(
                        name: "fk_dungeon_entrances_characters_character_id",
                        column: x => x.characterid,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dungeon_entrances_character_id",
                table: "dungeon_entrances",
                column: "character_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dungeon_entrances");
        }
    }
}
