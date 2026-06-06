using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Linkyou.System.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMenuMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Menus_TenantId_Name",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_TenantId_ParentId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_MenuRolePermissions_TenantId_MenuId_RoleName",
                table: "MenuRolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_MenuRolePermissions_TenantId_RoleName",
                table: "MenuRolePermissions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MenuRolePermissions");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_Name",
                table: "Menus",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRolePermissions_MenuId_RoleName",
                table: "MenuRolePermissions",
                columns: new[] { "MenuId", "RoleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuRolePermissions_RoleName",
                table: "MenuRolePermissions",
                column: "RoleName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Menus_Name",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_MenuRolePermissions_MenuId_RoleName",
                table: "MenuRolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_MenuRolePermissions_RoleName",
                table: "MenuRolePermissions");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Menus",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "MenuRolePermissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_TenantId_Name",
                table: "Menus",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_TenantId_ParentId",
                table: "Menus",
                columns: new[] { "TenantId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuRolePermissions_TenantId_MenuId_RoleName",
                table: "MenuRolePermissions",
                columns: new[] { "TenantId", "MenuId", "RoleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuRolePermissions_TenantId_RoleName",
                table: "MenuRolePermissions",
                columns: new[] { "TenantId", "RoleName" });
        }
    }
}
