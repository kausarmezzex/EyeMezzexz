using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class AddUserComputerAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_Computers_ComputerId",
                table: "TaskAssignments");

            migrationBuilder.RenameColumn(
                name: "ComputerId",
                table: "TaskAssignments",
                newName: "UserComputerAssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskAssignments_ComputerId",
                table: "TaskAssignments",
                newName: "IX_TaskAssignments_UserComputerAssignmentId");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "AssignedDuration",
                table: "TaskAssignments",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.CreateTable(
                name: "UserComputerAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ComputerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserComputerAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserComputerAssignments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserComputerAssignments_Computers_ComputerId",
                        column: x => x.ComputerId,
                        principalTable: "Computers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserComputerAssignments_ComputerId",
                table: "UserComputerAssignments",
                column: "ComputerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComputerAssignments_UserId",
                table: "UserComputerAssignments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_UserComputerAssignments_UserComputerAssignmentId",
                table: "TaskAssignments",
                column: "UserComputerAssignmentId",
                principalTable: "UserComputerAssignments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_UserComputerAssignments_UserComputerAssignmentId",
                table: "TaskAssignments");

            migrationBuilder.DropTable(
                name: "UserComputerAssignments");

            migrationBuilder.RenameColumn(
                name: "UserComputerAssignmentId",
                table: "TaskAssignments",
                newName: "ComputerId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskAssignments_UserComputerAssignmentId",
                table: "TaskAssignments",
                newName: "IX_TaskAssignments_ComputerId");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "AssignedDuration",
                table: "TaskAssignments",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_Computers_ComputerId",
                table: "TaskAssignments",
                column: "ComputerId",
                principalTable: "Computers",
                principalColumn: "Id");
        }
    }
}
