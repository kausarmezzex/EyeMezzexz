using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskTimer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskTimers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    TaskComment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StaffInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StaffOutTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTimers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTimers_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskTimers_TaskId",
                table: "TaskTimers",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskTimers");
        }
    }
}
