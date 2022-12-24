using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Game.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dungeons",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    requiredlevel = table.Column<int>(name: "required_level", type: "integer", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    cost = table.Column<long>(type: "bigint", nullable: false),
                    minexperience = table.Column<long>(name: "min_experience", type: "bigint", nullable: false),
                    maxexperience = table.Column<long>(name: "max_experience", type: "bigint", nullable: false),
                    mingold = table.Column<long>(name: "min_gold", type: "bigint", nullable: false),
                    maxgold = table.Column<long>(name: "max_gold", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dungeons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transactionid = table.Column<Guid>(name: "transaction_id", type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    rarity = table.Column<int>(type: "integer", nullable: false),
                    maxquality = table.Column<int>(name: "max_quality", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dungeon_entrances",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<int>(type: "integer", nullable: false),
                    processed = table.Column<bool>(type: "boolean", nullable: false),
                    charactertransactionid = table.Column<Guid>(name: "character_transaction_id", type: "uuid", nullable: false),
                    dungeonid = table.Column<long>(name: "dungeon_id", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dungeon_entrances", x => x.id);
                    table.ForeignKey(
                        name: "fk_dungeon_entrances_dungeons_dungeon_id",
                        column: x => x.dungeonid,
                        principalTable: "dungeons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "dungeon_journals",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wassuccessful = table.Column<bool>(name: "was_successful", type: "boolean", nullable: true),
                    elapsedmilliseconds = table.Column<long>(name: "elapsed_milliseconds", type: "bigint", nullable: false),
                    charactertransactionid = table.Column<Guid>(name: "character_transaction_id", type: "uuid", nullable: false),
                    dungeonid = table.Column<long>(name: "dungeon_id", type: "bigint", nullable: false),
                    rewardid = table.Column<long>(name: "reward_id", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dungeon_journals", x => x.id);
                    table.ForeignKey(
                        name: "fk_dungeon_journals_dungeons_dungeon_id",
                        column: x => x.dungeonid,
                        principalTable: "dungeons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dungeon_journals_items_reward_id",
                        column: x => x.rewardid,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dungeons_items",
                columns: table => new
                {
                    dungeonsid = table.Column<long>(name: "dungeons_id", type: "bigint", nullable: false),
                    rewardsid = table.Column<long>(name: "rewards_id", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dungeons_items", x => new { x.dungeonsid, x.rewardsid });
                    table.ForeignKey(
                        name: "fk_dungeons_items_dungeons_dungeons_id",
                        column: x => x.dungeonsid,
                        principalTable: "dungeons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dungeons_items_items_rewards_id",
                        column: x => x.rewardsid,
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

            migrationBuilder.CreateIndex(
                name: "ix_dungeon_entrances_dungeon_id",
                table: "dungeon_entrances",
                column: "dungeon_id");

            migrationBuilder.CreateIndex(
                name: "ix_dungeon_journals_dungeon_id",
                table: "dungeon_journals",
                column: "dungeon_id");

            migrationBuilder.CreateIndex(
                name: "ix_dungeon_journals_reward_id",
                table: "dungeon_journals",
                column: "reward_id");

            migrationBuilder.CreateIndex(
                name: "ix_dungeons_items_rewards_id",
                table: "dungeons_items",
                column: "rewards_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "armors");

            migrationBuilder.DropTable(
                name: "dungeon_entrances");

            migrationBuilder.DropTable(
                name: "dungeon_journals");

            migrationBuilder.DropTable(
                name: "dungeons_items");

            migrationBuilder.DropTable(
                name: "weapons");

            migrationBuilder.DropTable(
                name: "dungeons");

            migrationBuilder.DropTable(
                name: "items");
        }
    }
}
