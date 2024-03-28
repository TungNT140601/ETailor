using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Add_Field_ValueUsed_Table_ProductComponentMaterial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValueUsed",
                table: "OrderMaterial",
                type: "decimal(18,3)",
                nullable: true,
                defaultValueSql: "((0))");

            migrationBuilder.CreateTable(
                name: "OrderDashboard",
                columns: table => new
                {
                    Status = table.Column<int>(type: "int", nullable: true),
                    Total = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "StaffWithTotalTask",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fullname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: true),
                    TotalTask = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffWithTotalTask", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDashboard");

            migrationBuilder.DropTable(
                name: "StaffWithTotalTask");

            migrationBuilder.DropColumn(
                name: "ValueUsed",
                table: "OrderMaterial");
        }
    }
}
