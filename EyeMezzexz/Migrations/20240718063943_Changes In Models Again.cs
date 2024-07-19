using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInModelsAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_AspNetUsers_UserId",
                table: "Staffs");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskTimers_Tasks_TaskId",
                table: "TaskTimers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Permissions_PermissionId",
                table: "UserPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Staffs",
                table: "Staffs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions");

            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "TaskNames");

            migrationBuilder.RenameTable(
                name: "Staffs",
                newName: "StaffInOut");

            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "PermissionsName");

            migrationBuilder.RenameIndex(
                name: "IX_Staffs_UserId",
                table: "StaffInOut",
                newName: "IX_StaffInOut_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskNames",
                table: "TaskNames",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StaffInOut",
                table: "StaffInOut",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PermissionsName",
                table: "PermissionsName",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_PermissionsName_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "PermissionsName",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffInOut_AspNetUsers_UserId",
                table: "StaffInOut",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskTimers_TaskNames_TaskId",
                table: "TaskTimers",
                column: "TaskId",
                principalTable: "TaskNames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_PermissionsName_PermissionId",
                table: "UserPermissions",
                column: "PermissionId",
                principalTable: "PermissionsName",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_PermissionsName_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffInOut_AspNetUsers_UserId",
                table: "StaffInOut");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskTimers_TaskNames_TaskId",
                table: "TaskTimers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_PermissionsName_PermissionId",
                table: "UserPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskNames",
                table: "TaskNames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StaffInOut",
                table: "StaffInOut");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PermissionsName",
                table: "PermissionsName");

            migrationBuilder.RenameTable(
                name: "TaskNames",
                newName: "Tasks");

            migrationBuilder.RenameTable(
                name: "StaffInOut",
                newName: "Staffs");

            migrationBuilder.RenameTable(
                name: "PermissionsName",
                newName: "Permissions");

            migrationBuilder.RenameIndex(
                name: "IX_StaffInOut_UserId",
                table: "Staffs",
                newName: "IX_Staffs_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Staffs",
                table: "Staffs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_AspNetUsers_UserId",
                table: "Staffs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskTimers_Tasks_TaskId",
                table: "TaskTimers",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Permissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
