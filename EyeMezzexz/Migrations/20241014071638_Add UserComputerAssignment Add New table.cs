using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class AddUserComputerAssignmentAddNewtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "TaskAssignmentComputers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskAssignmentId = table.Column<int>(type: "int", nullable: false),
                    ComputerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignmentComputers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAssignmentComputers_Computers_ComputerId",
                        column: x => x.ComputerId,
                        principalTable: "Computers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskAssignmentComputers_TaskAssignments_TaskAssignmentId",
                        column: x => x.TaskAssignmentId,
                        principalTable: "TaskAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignmentComputers_ComputerId",
                table: "TaskAssignmentComputers",
                column: "ComputerId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignmentComputers_TaskAssignmentId",
                table: "TaskAssignmentComputers",
                column: "TaskAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskAssignmentComputers");

            migrationBuilder.AddColumn<int>(
                name: "ComputerId",
                table: "TaskAssignments",
                type: "int",
                nullable: true);

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
        }
    }
}
