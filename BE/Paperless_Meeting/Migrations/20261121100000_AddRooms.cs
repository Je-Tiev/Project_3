using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Paperless_Meeting.Migrations
{
    /// <inheritdoc />
    public partial class AddRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Equipment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                });

            migrationBuilder.AddColumn<int>(
                name: "PhysicalRoomId",
                table: "Meetings",
                type: "integer",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "Equipment", "Location", "RoomName", "Status" },
                values: new object[,]
                {
                    { 1, 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Màn hình LED, Hệ thống âm thanh, Camera 360", "Tòa nhà A - Tầng 2", "Conference Room A", 0 },
                    { 2, 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Máy chiếu, Bảng tương tác", "Tòa nhà B - Tầng 3", "Meeting Room 3", 1 },
                    { 3, 50, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hội nghị truyền hình 4K, Micro đại biểu", "Tòa nhà C - Tầng 5", "Boardroom", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_PhysicalRoomId",
                table: "Meetings",
                column: "PhysicalRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Rooms_PhysicalRoomId",
                table: "Meetings",
                column: "PhysicalRoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Rooms_PhysicalRoomId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_PhysicalRoomId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "PhysicalRoomId",
                table: "Meetings");

            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
