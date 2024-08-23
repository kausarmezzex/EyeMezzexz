using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class Add13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskTimers_AspNetUsers_UserId1",
                table: "TaskTimers");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskTimers_StaffInOut_UserId",
                table: "TaskTimers");

            migrationBuilder.DropIndex(
                name: "IX_TaskTimers_UserId1",
                table: "TaskTimers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "TaskTimers");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskTimers_AspNetUsers_UserId",
                table: "TaskTimers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskTimers_AspNetUsers_UserId",
                table: "TaskTimers");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "TaskTimers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TaskTimers_UserId1",
                table: "TaskTimers",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskTimers_AspNetUsers_UserId1",
                table: "TaskTimers",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskTimers_StaffInOut_UserId",
                table: "TaskTimers",
                column: "UserId",
                principalTable: "StaffInOut",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
