using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nucleuswfservice.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowAuditAndIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActorRole",
                table: "WorkflowHistoryEntry",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "WorkflowHistoryEntry",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "WorkflowHistoryEntry",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayloadSummary",
                table: "WorkflowHistoryEntry",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "TEXT", nullable: false),
                    Operation = table.Column<string>(type: "TEXT", nullable: false),
                    InstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DefinitionKey = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false),
                    Evento = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Operations_IdempotencyKey_Operation",
                table: "Operations",
                columns: new[] { "IdempotencyKey", "Operation" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropColumn(
                name: "ActorRole",
                table: "WorkflowHistoryEntry");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "WorkflowHistoryEntry");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "WorkflowHistoryEntry");

            migrationBuilder.DropColumn(
                name: "PayloadSummary",
                table: "WorkflowHistoryEntry");
        }
    }
}
