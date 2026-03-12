using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace personalservice.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Legajos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", nullable: false),
                    Documento = table.Column<string>(type: "TEXT", nullable: false),
                    Cuil = table.Column<string>(type: "TEXT", nullable: false),
                    EstadoCivil = table.Column<string>(type: "TEXT", nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Convenio = table.Column<string>(type: "TEXT", nullable: true),
                    Categoria = table.Column<string>(type: "TEXT", nullable: true),
                    ObraSocial = table.Column<string>(type: "TEXT", nullable: true),
                    Sindicato = table.Column<string>(type: "TEXT", nullable: true),
                    TipoPersonal = table.Column<string>(type: "TEXT", nullable: true),
                    Ubicacion = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Legajos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Familiares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", nullable: false),
                    Documento = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Vive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Discapacidad = table.Column<bool>(type: "INTEGER", nullable: false),
                    ACargo = table.Column<bool>(type: "INTEGER", nullable: false),
                    ACargoObraSocial = table.Column<bool>(type: "INTEGER", nullable: false),
                    AplicaGanancias = table.Column<bool>(type: "INTEGER", nullable: false),
                    LegajoId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familiares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Familiares_Legajos_LegajoId",
                        column: x => x.LegajoId,
                        principalTable: "Legajos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Licencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    CodigoSIJP = table.Column<string>(type: "TEXT", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConGoce = table.Column<bool>(type: "INTEGER", nullable: false),
                    CuentaVacaciones = table.Column<bool>(type: "INTEGER", nullable: false),
                    LegajoId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Licencias_Legajos_LegajoId",
                        column: x => x.LegajoId,
                        principalTable: "Legajos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Familiares_LegajoId",
                table: "Familiares",
                column: "LegajoId");

            migrationBuilder.CreateIndex(
                name: "IX_Legajos_Numero",
                table: "Legajos",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licencias_LegajoId",
                table: "Licencias",
                column: "LegajoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Familiares");

            migrationBuilder.DropTable(
                name: "Licencias");

            migrationBuilder.DropTable(
                name: "Legajos");
        }
    }
}
