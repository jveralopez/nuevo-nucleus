using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace organizacionservice.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganigramas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organigramas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    UnidadesJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organigramas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organigramas_EmpresaId_Version",
                table: "Organigramas",
                columns: new[] { "EmpresaId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organigramas");
        }
    }
}
