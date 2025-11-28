using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Paperless_Meeting.Migrations
{
    /// <inheritdoc />
    public partial class add_roomid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoomId",
                table: "Meetings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Meetings");
        }
    }
}
