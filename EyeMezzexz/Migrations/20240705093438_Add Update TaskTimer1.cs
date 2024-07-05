using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateTaskTimer1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TaskTimers_UserId",
                table: "TaskTimers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskTimers_Users_UserId",
                table: "TaskTimers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskTimers_Users_UserId",
                table: "TaskTimers");

            migrationBuilder.DropIndex(
                name: "IX_TaskTimers_UserId",
                table: "TaskTimers");
        }
    }
}
