using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Change_Table_ProductComponentMaterial_To_ProductStageMaterial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__ProductCo__Produ__31D75E8D",
                table: "ProductComponentMaterial");

            migrationBuilder.RenameColumn(
                name: "ProductComponentId",
                table: "ProductComponentMaterial",
                newName: "ProductStageId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductComponentMaterial_ProductComponentId",
                table: "ProductComponentMaterial",
                newName: "IX_ProductComponentMaterial_ProductStageId");

            migrationBuilder.AddForeignKey(
                name: "FK__ProductStage__Materail__31D75E8D",
                table: "ProductComponentMaterial",
                column: "ProductStageId",
                principalTable: "ProductStage",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__ProductStage__Materail__31D75E8D",
                table: "ProductComponentMaterial");

            migrationBuilder.RenameColumn(
                name: "ProductStageId",
                table: "ProductComponentMaterial",
                newName: "ProductComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductComponentMaterial_ProductStageId",
                table: "ProductComponentMaterial",
                newName: "IX_ProductComponentMaterial_ProductComponentId");

            migrationBuilder.AddForeignKey(
                name: "FK__ProductCo__Produ__31D75E8D",
                table: "ProductComponentMaterial",
                column: "ProductComponentId",
                principalTable: "ProductComponent",
                principalColumn: "Id");
        }
    }
}
