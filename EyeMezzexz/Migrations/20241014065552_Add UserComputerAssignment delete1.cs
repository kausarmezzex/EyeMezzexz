using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class AddUserComputerAssignmentdelete1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_UserComputerAssignment_UserComputerAssignmentId",
                table: "TaskAssignments");

            migrationBuilder.DropTable(
                name: "UserComputerAssignment");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_UserComputerAssignmentId",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "UserComputerAssignmentId",
                table: "TaskAssignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserComputerAssignmentId",
                table: "TaskAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserComputerAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComputerId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserComputerAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserComputerAssignment_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserComputerAssignment_Computers_ComputerId",
                        column: x => x.ComputerId,
                        principalTable: "Computers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_UserComputerAssignmentId",
                table: "TaskAssignments",
                column: "UserComputerAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComputerAssignment_ComputerId",
                table: "UserComputerAssignment",
                column: "ComputerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComputerAssignment_UserId",
                table: "UserComputerAssignment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_UserComputerAssignment_UserComputerAssignmentId",
                table: "TaskAssignments",
                column: "UserComputerAssignmentId",
                principalTable: "UserComputerAssignment",
                principalColumn: "Id");
        }
    }
}
