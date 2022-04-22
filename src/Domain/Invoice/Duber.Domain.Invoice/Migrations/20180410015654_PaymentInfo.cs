using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Duber.Domain.Invoice.Migrations
{
    public partial class PaymentInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentsInfo",
                columns: table => new
                {
                    Status = table.Column<int>(nullable: false),
                    CardNumber = table.Column<string>(nullable: false),
                    CardType = table.Column<string>(nullable: false),
                    InvoiceId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsInfo", x => new { x.Status, x.CardNumber, x.CardType, x.InvoiceId, x.UserId });
                    table.ForeignKey(
                        name: "FK_PaymentsInfo_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsInfo_InvoiceId",
                table: "PaymentsInfo",
                column: "InvoiceId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentsInfo");
        }
    }
}
