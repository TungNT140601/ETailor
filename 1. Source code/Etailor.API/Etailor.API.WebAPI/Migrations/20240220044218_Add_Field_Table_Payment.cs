using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Add_Field_Table_Payment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountAfterRefund",
                table: "Payment",
                type: "decimal(18,0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentRefundId",
                table: "Payment",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentRefundId",
                table: "Payment",
                column: "PaymentRefundId");

            migrationBuilder.AddForeignKey(
                name: "FK__Payment__PaymentRefund__1A25A48",
                table: "Payment",
                column: "PaymentRefundId",
                principalTable: "Payment",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Payment__PaymentRefund__1A25A48",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_PaymentRefundId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "AmountAfterRefund",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PaymentRefundId",
                table: "Payment");
        }
    }
}
