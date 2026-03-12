using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace liquidacionservice.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payrolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Periodo = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payrolls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegajoEnLote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Cuil = table.Column<string>(type: "TEXT", nullable: false),
                    Convenio = table.Column<string>(type: "TEXT", nullable: true),
                    Categoria = table.Column<string>(type: "TEXT", nullable: true),
                    Basico = table.Column<decimal>(type: "TEXT", nullable: false),
                    Antiguedad = table.Column<decimal>(type: "TEXT", nullable: false),
                    Adicionales = table.Column<decimal>(type: "TEXT", nullable: false),
                    Presentismo = table.Column<decimal>(type: "TEXT", nullable: false),
                    HorasExtra = table.Column<decimal>(type: "TEXT", nullable: false),
                    Premios = table.Column<decimal>(type: "TEXT", nullable: false),
                    Descuentos = table.Column<decimal>(type: "TEXT", nullable: false),
                    NoRemunerativo = table.Column<decimal>(type: "TEXT", nullable: false),
                    BonosNoRemunerativos = table.Column<decimal>(type: "TEXT", nullable: false),
                    AplicaGanancias = table.Column<bool>(type: "INTEGER", nullable: false),
                    OmitirGanancias = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConyugeACargo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CantHijos = table.Column<int>(type: "INTEGER", nullable: false),
                    CantOtrosFamiliares = table.Column<int>(type: "INTEGER", nullable: false),
                    DeduccionesAdicionales = table.Column<decimal>(type: "TEXT", nullable: false),
                    VacacionesDias = table.Column<int>(type: "INTEGER", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegajoEnLote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegajoEnLote_Payrolls_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "Payrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollReceipt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LegajoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LegajoNumero = table.Column<string>(type: "TEXT", nullable: false),
                    LegajoNombre = table.Column<string>(type: "TEXT", nullable: false),
                    Remunerativo = table.Column<decimal>(type: "TEXT", nullable: false),
                    Deducciones = table.Column<decimal>(type: "TEXT", nullable: false),
                    Neto = table.Column<decimal>(type: "TEXT", nullable: false),
                    GeneradoEn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollReceipt_Payrolls_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "Payrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Embargo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "TEXT", nullable: true),
                    MontoFijo = table.Column<decimal>(type: "TEXT", nullable: true),
                    MontoTotal = table.Column<decimal>(type: "TEXT", nullable: true),
                    MontoPendiente = table.Column<decimal>(type: "TEXT", nullable: true),
                    BaseCalculo = table.Column<string>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    LegajoEnLoteId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Embargo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Embargo_LegajoEnLote_LegajoEnLoteId",
                        column: x => x.LegajoEnLoteId,
                        principalTable: "LegajoEnLote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicenciaEnLote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Dias = table.Column<int>(type: "INTEGER", nullable: false),
                    ConGoce = table.Column<bool>(type: "INTEGER", nullable: false),
                    LegajoEnLoteId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenciaEnLote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenciaEnLote_LegajoEnLote_LegajoEnLoteId",
                        column: x => x.LegajoEnLoteId,
                        principalTable: "LegajoEnLote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Concepto = table.Column<string>(type: "TEXT", nullable: false),
                    Importe = table.Column<decimal>(type: "TEXT", nullable: false),
                    PayrollReceiptId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptDetail_PayrollReceipt_PayrollReceiptId",
                        column: x => x.PayrollReceiptId,
                        principalTable: "PayrollReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Embargo_LegajoEnLoteId",
                table: "Embargo",
                column: "LegajoEnLoteId");

            migrationBuilder.CreateIndex(
                name: "IX_LegajoEnLote_PayrollRunId",
                table: "LegajoEnLote",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenciaEnLote_LegajoEnLoteId",
                table: "LicenciaEnLote",
                column: "LegajoEnLoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollReceipt_PayrollRunId",
                table: "PayrollReceipt",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptDetail_PayrollReceiptId",
                table: "ReceiptDetail",
                column: "PayrollReceiptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Embargo");

            migrationBuilder.DropTable(
                name: "LicenciaEnLote");

            migrationBuilder.DropTable(
                name: "ReceiptDetail");

            migrationBuilder.DropTable(
                name: "LegajoEnLote");

            migrationBuilder.DropTable(
                name: "PayrollReceipt");

            migrationBuilder.DropTable(
                name: "Payrolls");
        }
    }
}
