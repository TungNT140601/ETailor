using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Add_Field_FabricMaterialId_ReferenceProfileBodyId_Table_Product : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FabricMaterialId",
                table: "Product",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceProfileBodyId",
                table: "Product",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_FabricMaterialId",
                table: "Product",
                column: "FabricMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ReferenceProfileBodyId",
                table: "Product",
                column: "ReferenceProfileBodyId");

            migrationBuilder.AddForeignKey(
                name: "FK__Product__Fabric_Material__6B44E613",
                table: "Product",
                column: "FabricMaterialId",
                principalTable: "Material",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__Product__ProfileBody__6B44E613",
                table: "Product",
                column: "ReferenceProfileBodyId",
                principalTable: "ProfileBody",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Product__Fabric_Material__6B44E613",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK__Product__ProfileBody__6B44E613",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_FabricMaterialId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_ReferenceProfileBodyId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "FabricMaterialId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ReferenceProfileBodyId",
                table: "Product");
        }
    }
}
