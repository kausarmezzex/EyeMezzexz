using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskTimer1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "TotalWorkingTime",
                table: "TaskTimers",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalWorkingTime",
                table: "TaskTimers");
        }
    }
}
