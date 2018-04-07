using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Duber.Domain.Invoice.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Distance = table.Column<double>(nullable: false),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    Fee = table.Column<decimal>(nullable: false),
                    PaymentMethodId = table.Column<int>(nullable: false),
                    Total = table.Column<decimal>(nullable: false),
                    TripId = table.Column<Guid>(nullable: false),
                    TripStatusId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");
        }
    }
}
