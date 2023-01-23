using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Migrations
{
    /// <inheritdoc />
    public partial class OptRewardJournal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_dungeon_journals_items_reward_id",
                table: "dungeon_journals");

            migrationBuilder.AlterColumn<long>(
                name: "reward_id",
                table: "dungeon_journals",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "fk_dungeon_journals_items_reward_id",
                table: "dungeon_journals",
                column: "reward_id",
                principalTable: "items",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_dungeon_journals_items_reward_id",
                table: "dungeon_journals");

            migrationBuilder.AlterColumn<long>(
                name: "reward_id",
                table: "dungeon_journals",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_dungeon_journals_items_reward_id",
                table: "dungeon_journals",
                column: "reward_id",
                principalTable: "items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
