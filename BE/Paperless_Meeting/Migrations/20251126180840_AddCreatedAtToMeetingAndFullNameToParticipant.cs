using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Paperless_Meeting.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToMeetingAndFullNameToParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conclusions_Users_ConclusionByUserId",
                table: "Conclusions");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_UploadedBy",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingAudios_Users_UserId",
                table: "MeetingAudios");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingLogs_Users_UserId",
                table: "MeetingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingParticipants_Users_UserId",
                table: "MeetingParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users_CreatedByUserId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Users_UserId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Polls_Users_VoteByUserId",
                table: "Polls");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVotes_Users_UserId",
                table: "UserVotes");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Notes");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Meetings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "MeetingParticipants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MeetingParticipantId",
                table: "MeetingParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Conclusions_Users_ConclusionByUserId",
                table: "Conclusions",
                column: "ConclusionByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_UploadedBy",
                table: "Documents",
                column: "UploadedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingAudios_Users_UserId",
                table: "MeetingAudios",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingLogs_Users_UserId",
                table: "MeetingLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingParticipants_Users_UserId",
                table: "MeetingParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users_CreatedByUserId",
                table: "Meetings",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Users_UserId",
                table: "Notes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_Users_VoteByUserId",
                table: "Polls",
                column: "VoteByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVotes_Users_UserId",
                table: "UserVotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conclusions_Users_ConclusionByUserId",
                table: "Conclusions");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_UploadedBy",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingAudios_Users_UserId",
                table: "MeetingAudios");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingLogs_Users_UserId",
                table: "MeetingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingParticipants_Users_UserId",
                table: "MeetingParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users_CreatedByUserId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Users_UserId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Polls_Users_VoteByUserId",
                table: "Polls");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVotes_Users_UserId",
                table: "UserVotes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "MeetingParticipants");

            migrationBuilder.DropColumn(
                name: "MeetingParticipantId",
                table: "MeetingParticipants");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Notes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Conclusions_Users_ConclusionByUserId",
                table: "Conclusions",
                column: "ConclusionByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_UploadedBy",
                table: "Documents",
                column: "UploadedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingAudios_Users_UserId",
                table: "MeetingAudios",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingLogs_Users_UserId",
                table: "MeetingLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingParticipants_Users_UserId",
                table: "MeetingParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users_CreatedByUserId",
                table: "Meetings",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Users_UserId",
                table: "Notes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Polls_Users_VoteByUserId",
                table: "Polls",
                column: "VoteByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVotes_Users_UserId",
                table: "UserVotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
