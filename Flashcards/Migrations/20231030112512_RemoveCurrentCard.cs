using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flashcards.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCurrentCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Cards_CurrentCardId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_CurrentCardId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CurrentCardId",
                table: "Sessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentCardId",
                table: "Sessions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CurrentCardId",
                table: "Sessions",
                column: "CurrentCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Cards_CurrentCardId",
                table: "Sessions",
                column: "CurrentCardId",
                principalTable: "Cards",
                principalColumn: "CardId");
        }
    }
}
