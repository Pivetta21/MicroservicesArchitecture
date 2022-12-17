using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Armory.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    size = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transactionid = table.Column<Guid>(name: "transaction_id", type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    rarity = table.Column<int>(type: "integer", nullable: false),
                    inventoryid = table.Column<long>(name: "inventory_id", type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_items_inventories_inventory_id",
                        column: x => x.inventoryid,
                        principalTable: "inventories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "armors",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    resistance = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_armors", x => x.id);
                    table.ForeignKey(
                        name: "fk_armors_items_id",
                        column: x => x.id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "weapons",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    power = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weapons", x => x.id);
                    table.ForeignKey(
                        name: "fk_weapons_items_id",
                        column: x => x.id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "builds",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    weaponid = table.Column<long>(name: "weapon_id", type: "bigint", nullable: false),
                    armorid = table.Column<long>(name: "armor_id", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_builds", x => x.id);
                    table.ForeignKey(
                        name: "fk_builds_items_armor_id",
                        column: x => x.armorid,
                        principalTable: "armors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_builds_items_weapon_id",
                        column: x => x.weaponid,
                        principalTable: "weapons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transactionid = table.Column<Guid>(name: "transaction_id", type: "uuid", nullable: false),
                    usertransactionid = table.Column<Guid>(name: "user_transaction_id", type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    specialization = table.Column<int>(type: "integer", nullable: false),
                    life = table.Column<int>(type: "integer", nullable: false),
                    damage = table.Column<int>(type: "integer", nullable: false),
                    gold = table.Column<long>(type: "bigint", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    experience = table.Column<double>(type: "double precision", nullable: false),
                    isplaying = table.Column<bool>(name: "is_playing", type: "boolean", nullable: false),
                    buildid = table.Column<long>(name: "build_id", type: "bigint", nullable: false),
                    inventoryid = table.Column<long>(name: "inventory_id", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_characters", x => x.id);
                    table.ForeignKey(
                        name: "fk_characters_builds_build_id",
                        column: x => x.buildid,
                        principalTable: "builds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_characters_inventories_inventory_id",
                        column: x => x.inventoryid,
                        principalTable: "inventories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_builds_armor_id",
                table: "builds",
                column: "armor_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_builds_weapon_id",
                table: "builds",
                column: "weapon_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_characters_build_id",
                table: "characters",
                column: "build_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_characters_inventory_id",
                table: "characters",
                column: "inventory_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_items_inventory_id",
                table: "items",
                column: "inventory_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "characters");

            migrationBuilder.DropTable(
                name: "builds");

            migrationBuilder.DropTable(
                name: "armors");

            migrationBuilder.DropTable(
                name: "weapons");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "inventories");
        }
    }
}
