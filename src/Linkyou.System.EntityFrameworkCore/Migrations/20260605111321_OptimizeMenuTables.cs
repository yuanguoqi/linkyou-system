using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Linkyou.System.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeMenuTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MenuRolePermissions_TenantId_MenuId_RoleName",
                table: "MenuRolePermissions");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_ParentId",
                table: "Menus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_Sort",
                table: "Menus",
                column: "Sort");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_TenantId_ParentId",
                table: "Menus",
                columns: new[] { "TenantId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuRolePermissions_MenuId",
                table: "MenuRolePermissions",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRolePermissions_TenantId_MenuId_RoleName",
                table: "MenuRolePermissions",
                columns: new[] { "TenantId", "MenuId", "RoleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuRolePermissions_TenantId_RoleName",
                table: "MenuRolePermissions",
                columns: new[] { "TenantId", "RoleName" });

            migrationBuilder.AddForeignKey(
                name: "FK_MenuRolePermissions_MenuId",
                table: "MenuRolePermissions",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_ParentId",
                table: "Menus",
                column: "ParentId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuRolePermissions_MenuId",
                table: "MenuRolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_ParentId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_ParentId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_Sort",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_TenantId_ParentId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_MenuRolePermissions_MenuId",
                table: "MenuRolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_MenuRolePermissions_TenantId_MenuId_RoleName",
                table: "MenuRolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_MenuRolePermissions_TenantId_RoleName",
                table: "MenuRolePermissions");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRolePermissions_TenantId_MenuId_RoleName",
                table: "MenuRolePermissions",
                columns: new[] { "TenantId", "MenuId", "RoleName" });
        }
    }
}
