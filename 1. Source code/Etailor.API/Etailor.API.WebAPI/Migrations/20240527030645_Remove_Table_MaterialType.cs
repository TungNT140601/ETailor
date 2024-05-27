using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Remove_Table_MaterialType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__MaterialC__Mater__7C6F7215",
                table: "MaterialCategory");

            migrationBuilder.DropTable(
                name: "MaterialType");

            migrationBuilder.DropIndex(
                name: "IX_MaterialCategory_MaterialTypeId",
                table: "MaterialCategory");

            migrationBuilder.DropColumn(
                name: "MaterialTypeId",
                table: "MaterialCategory");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaterialTypeId",
                table: "MaterialCategory",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MaterialType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    LastestUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialCategory_MaterialTypeId",
                table: "MaterialCategory",
                column: "MaterialTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK__MaterialC__Mater__7C6F7215",
                table: "MaterialCategory",
                column: "MaterialTypeId",
                principalTable: "MaterialType",
                principalColumn: "Id");
        }
    }
}
