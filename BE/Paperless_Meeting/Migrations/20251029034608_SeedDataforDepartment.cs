using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Paperless_Meeting.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataforDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MeetingParticipants",
                table: "MeetingParticipants");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeetingParticipants",
                table: "MeetingParticipants",
                columns: new[] { "MeetingId", "UserId" });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "DepartmentId", "DepartmentName" },
                values: new object[,]
                {
                    { 1, "Phòng Bảo vệ" },
                    { 2, "Phòng Kỹ thuật" },
                    { 3, "Phòng Kế toán" },
                    { 4, "Ban Giám đốc" },
                    { 5, "Phòng Nhân sự" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MeetingParticipants",
                table: "MeetingParticipants");

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 5);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeetingParticipants",
                table: "MeetingParticipants",
                column: "MeetingId");
        }
    }
}
