using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class AddUserComputerAssignmentdelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_UserComputerAssignments_UserComputerAssignmentId",
                table: "TaskAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserComputerAssignments_AspNetUsers_UserId",
                table: "UserComputerAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserComputerAssignments_Computers_ComputerId",
                table: "UserComputerAssignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserComputerAssignments",
                table: "UserComputerAssignments");

            migrationBuilder.RenameTable(
                name: "UserComputerAssignments",
                newName: "UserComputerAssignment");

            migrationBuilder.RenameIndex(
                name: "IX_UserComputerAssignments_UserId",
                table: "UserComputerAssignment",
                newName: "IX_UserComputerAssignment_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserComputerAssignments_ComputerId",
                table: "UserComputerAssignment",
                newName: "IX_UserComputerAssignment_ComputerId");

            migrationBuilder.AddColumn<int>(
                name: "ComputerId",
                table: "TaskAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserComputerAssignment",
                table: "UserComputerAssignment",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_ComputerId",
                table: "TaskAssignments",
                column: "ComputerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_Computers_ComputerId",
                table: "TaskAssignments",
                column: "ComputerId",
                principalTable: "Computers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_UserComputerAssignment_UserComputerAssignmentId",
                table: "TaskAssignments",
                column: "UserComputerAssignmentId",
                principalTable: "UserComputerAssignment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserComputerAssignment_AspNetUsers_UserId",
                table: "UserComputerAssignment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserComputerAssignment_Computers_ComputerId",
                table: "UserComputerAssignment",
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

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_UserComputerAssignment_UserComputerAssignmentId",
                table: "TaskAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserComputerAssignment_AspNetUsers_UserId",
                table: "UserComputerAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_UserComputerAssignment_Computers_ComputerId",
                table: "UserComputerAssignment");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_ComputerId",
                table: "TaskAssignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserComputerAssignment",
                table: "UserComputerAssignment");

            migrationBuilder.DropColumn(
                name: "ComputerId",
                table: "TaskAssignments");

            migrationBuilder.RenameTable(
                name: "UserComputerAssignment",
                newName: "UserComputerAssignments");

            migrationBuilder.RenameIndex(
                name: "IX_UserComputerAssignment_UserId",
                table: "UserComputerAssignments",
                newName: "IX_UserComputerAssignments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserComputerAssignment_ComputerId",
                table: "UserComputerAssignments",
                newName: "IX_UserComputerAssignments_ComputerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserComputerAssignments",
                table: "UserComputerAssignments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_UserComputerAssignments_UserComputerAssignmentId",
                table: "TaskAssignments",
                column: "UserComputerAssignmentId",
                principalTable: "UserComputerAssignments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserComputerAssignments_AspNetUsers_UserId",
                table: "UserComputerAssignments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserComputerAssignments_Computers_ComputerId",
                table: "UserComputerAssignments",
                column: "ComputerId",
                principalTable: "Computers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
