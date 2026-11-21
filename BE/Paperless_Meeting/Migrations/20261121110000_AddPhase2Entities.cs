using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Paperless_Meeting.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase2Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSecret",
                table: "Polls",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Notes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_MeetingId_UserId",
                table: "Notes",
                columns: new[] { "MeetingId", "UserId" },
                unique: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Conclusions",
                type: "text",
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddColumn<string>(
                name: "SignedByUsers",
                table: "Conclusions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Conclusions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeetingChats",
                columns: table => new
                {
                    ChatId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MeetingId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingChats", x => x.ChatId);
                    table.ForeignKey(
                        name: "FK_MeetingChats_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "MeetingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingChats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingChats_MeetingId",
                table: "MeetingChats",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingChats_UserId",
                table: "MeetingChats",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingChats");

            migrationBuilder.DropIndex(
                name: "IX_Notes_MeetingId_UserId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "IsSecret",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Conclusions");

            migrationBuilder.DropColumn(
                name: "SignedByUsers",
                table: "Conclusions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Conclusions");
        }
    }
}
