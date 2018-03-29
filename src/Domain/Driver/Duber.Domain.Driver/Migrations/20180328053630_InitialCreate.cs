using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Duber.Domain.Driver.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Driver");

            migrationBuilder.CreateSequence(
                name: "driverseq",
                schema: "Driver",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "vehicleseq",
                schema: "Driver",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "DriverStatuses",
                schema: "Driver",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValue: 1),
                    Name = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                schema: "Driver",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValue: 1),
                    Name = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                schema: "Driver",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: true),
                    Rating = table.Column<int>(nullable: false),
                    StatusId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drivers_DriverStatuses_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Driver",
                        principalTable: "DriverStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                schema: "Driver",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    Brand = table.Column<string>(nullable: false),
                    DriverId = table.Column<int>(nullable: false),
                    Model = table.Column<string>(nullable: false),
                    Plate = table.Column<string>(nullable: false),
                    TypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "Driver",
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicles_VehicleTypes_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "Driver",
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_Email",
                schema: "Driver",
                table: "Drivers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_StatusId",
                schema: "Driver",
                table: "Drivers",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_DriverId",
                schema: "Driver",
                table: "Vehicles",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TypeId",
                schema: "Driver",
                table: "Vehicles",
                column: "TypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vehicles",
                schema: "Driver");

            migrationBuilder.DropTable(
                name: "Drivers",
                schema: "Driver");

            migrationBuilder.DropTable(
                name: "VehicleTypes",
                schema: "Driver");

            migrationBuilder.DropTable(
                name: "DriverStatuses",
                schema: "Driver");

            migrationBuilder.DropSequence(
                name: "driverseq",
                schema: "Driver");

            migrationBuilder.DropSequence(
                name: "vehicleseq",
                schema: "Driver");
        }
    }
}
