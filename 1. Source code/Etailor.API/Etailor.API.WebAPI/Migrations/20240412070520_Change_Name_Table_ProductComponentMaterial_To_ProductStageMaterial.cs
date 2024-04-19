using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Change_Name_Table_ProductComponentMaterial_To_ProductStageMaterial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductComponentMaterial",
                table: "ProductComponentMaterial");

            migrationBuilder.RenameTable(
                name: "ProductComponentMaterial",
                newName: "ProductStageMaterial");

            migrationBuilder.RenameIndex(
                name: "IX_ProductComponentMaterial_ProductStageId",
                table: "ProductStageMaterial",
                newName: "IX_ProductStageMaterial_ProductStageId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductComponentMaterial_MaterialId",
                table: "ProductStageMaterial",
                newName: "IX_ProductStageMaterial_MaterialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductStageMaterial",
                table: "ProductStageMaterial",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductStageMaterial",
                table: "ProductStageMaterial");

            migrationBuilder.RenameTable(
                name: "ProductStageMaterial",
                newName: "ProductComponentMaterial");

            migrationBuilder.RenameIndex(
                name: "IX_ProductStageMaterial_ProductStageId",
                table: "ProductComponentMaterial",
                newName: "IX_ProductComponentMaterial_ProductStageId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductStageMaterial_MaterialId",
                table: "ProductComponentMaterial",
                newName: "IX_ProductComponentMaterial_MaterialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductComponentMaterial",
                table: "ProductComponentMaterial",
                column: "Id");
        }
    }
}
