using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tiemposservice.Migrations
{
    /// <inheritdoc />
    public partial class AddAusencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ausencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LegajoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LegajoNumero = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    FechaDesde = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FechaHasta = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Origen = table.Column<string>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ausencias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ausencias_FechaDesde",
                table: "Ausencias",
                column: "FechaDesde");

            migrationBuilder.CreateIndex(
                name: "IX_Ausencias_FechaHasta",
                table: "Ausencias",
                column: "FechaHasta");

            migrationBuilder.CreateIndex(
                name: "IX_Ausencias_LegajoId",
                table: "Ausencias",
                column: "LegajoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ausencias_LegajoNumero",
                table: "Ausencias",
                column: "LegajoNumero");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ausencias");
        }
    }
}
