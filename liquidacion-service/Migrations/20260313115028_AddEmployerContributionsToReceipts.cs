using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace liquidacionservice.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployerContributionsToReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ContribucionesPatronalesTotal",
                table: "PayrollReceipt",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "EmployerContribution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Concepto = table.Column<string>(type: "TEXT", nullable: false),
                    Importe = table.Column<decimal>(type: "TEXT", nullable: false),
                    Grupo = table.Column<string>(type: "TEXT", nullable: true),
                    PayrollReceiptId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployerContribution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployerContribution_PayrollReceipt_PayrollReceiptId",
                        column: x => x.PayrollReceiptId,
                        principalTable: "PayrollReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LegajoEmployerContribution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Concepto = table.Column<string>(type: "TEXT", nullable: false),
                    Importe = table.Column<decimal>(type: "TEXT", nullable: false),
                    Grupo = table.Column<string>(type: "TEXT", nullable: true),
                    LegajoEnLoteId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegajoEmployerContribution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegajoEmployerContribution_LegajoEnLote_LegajoEnLoteId",
                        column: x => x.LegajoEnLoteId,
                        principalTable: "LegajoEnLote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployerContribution_PayrollReceiptId",
                table: "EmployerContribution",
                column: "PayrollReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_LegajoEmployerContribution_LegajoEnLoteId",
                table: "LegajoEmployerContribution",
                column: "LegajoEnLoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployerContribution");

            migrationBuilder.DropTable(
                name: "LegajoEmployerContribution");

            migrationBuilder.DropColumn(
                name: "ContribucionesPatronalesTotal",
                table: "PayrollReceipt");
        }
    }
}
