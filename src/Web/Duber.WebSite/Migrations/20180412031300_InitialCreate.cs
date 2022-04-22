using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Duber.WebSite.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Reporting");

            migrationBuilder.CreateTable(
                name: "Trips",
                schema: "Reporting",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Brand = table.Column<string>(nullable: true),
                    CardNumber = table.Column<string>(nullable: true),
                    CardType = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Distance = table.Column<double>(nullable: true),
                    DriverId = table.Column<int>(nullable: false),
                    DriverName = table.Column<string>(nullable: true),
                    Duration = table.Column<TimeSpan>(nullable: true),
                    Ended = table.Column<DateTime>(nullable: true),
                    Fare = table.Column<decimal>(nullable: true),
                    Fee = table.Column<decimal>(nullable: true),
                    From = table.Column<string>(nullable: true),
                    InvoiceId = table.Column<Guid>(nullable: true),
                    Model = table.Column<string>(nullable: true),
                    PaymentMethod = table.Column<string>(nullable: true),
                    PaymentStatus = table.Column<string>(nullable: true),
                    Plate = table.Column<string>(nullable: true),
                    Started = table.Column<DateTime>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    To = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trips",
                schema: "Reporting");
        }
    }
}
