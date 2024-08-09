using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCountryField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskNames_Computers_ComputerId",
                table: "TaskNames");

            migrationBuilder.DropIndex(
                name: "IX_TaskNames_ComputerId",
                table: "TaskNames");

            migrationBuilder.DropColumn(
                name: "ComputerId",
                table: "TaskNames");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TaskNames",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TaskNames");

            migrationBuilder.AddColumn<int>(
                name: "ComputerId",
                table: "TaskNames",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskNames_ComputerId",
                table: "TaskNames",
                column: "ComputerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskNames_Computers_ComputerId",
                table: "TaskNames",
                column: "ComputerId",
                principalTable: "Computers",
                principalColumn: "Id");
        }
    }
}
