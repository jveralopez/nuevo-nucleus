using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace personalservice.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitudesCambio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesCambio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LegajoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", nullable: true),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    DatosJson = table.Column<string>(type: "TEXT", nullable: true),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesCambio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesCambio_Legajos_LegajoId",
                        column: x => x.LegajoId,
                        principalTable: "Legajos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCambio_Estado",
                table: "SolicitudesCambio",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCambio_LegajoId",
                table: "SolicitudesCambio",
                column: "LegajoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesCambio");
        }
    }
}
