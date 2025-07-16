using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionalOutbox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogTailingOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    PayloadType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogTailingOutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PollingOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    PayloadType = table.Column<string>(type: "text", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollingOutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollingOutboxMessages_ProcessedDate_ProcessedCount_Creation~",
                table: "PollingOutboxMessages",
                columns: ["ProcessedDate", "ProcessedCount", "CreationDate"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogTailingOutboxMessages");

            migrationBuilder.DropTable(
                name: "PollingOutboxMessages");
        }
    }
}
