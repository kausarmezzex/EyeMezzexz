using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class UpdatetaskAsssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComputerId",
                table: "TaskAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "TaskAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TargetQuantity",
                table: "TaskAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_ComputerId",
                table: "TaskAssignments",
                column: "ComputerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_Computers_ComputerId",
                table: "TaskAssignments",
                column: "ComputerId",
                principalTable: "Computers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_Computers_ComputerId",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_ComputerId",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "ComputerId",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "TargetQuantity",
                table: "TaskAssignments");
        }
    }
}
