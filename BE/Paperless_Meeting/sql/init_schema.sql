BEGIN;

CREATE TABLE "Departments" (
  "DepartmentId" serial PRIMARY KEY,
  "DepartmentName" varchar(100) NOT NULL
);

CREATE TABLE "Users" (
  "UserId" serial PRIMARY KEY,
  "FullName" varchar(100) NOT NULL,
  "Username" varchar(50) NOT NULL,
  "PasswordHash" text NOT NULL,
  "Email" varchar(100) NOT NULL,
  "Role" text NOT NULL,
  "DepartmentId" integer NOT NULL,
  CONSTRAINT "FK_Users_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("DepartmentId") ON DELETE CASCADE
);

CREATE TABLE "Meetings" (
  "MeetingId" serial PRIMARY KEY,
  "Title" varchar(200) NOT NULL,
  "Description" varchar(1000),
  "StartTime" timestamp with time zone NOT NULL,
  "EndTime" timestamp with time zone NOT NULL,
  "Location" varchar(200) NOT NULL,
  "CreatedByUserId" integer NOT NULL,
  "Status" text NOT NULL,
  CONSTRAINT "FK_Meetings_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

CREATE TABLE "RefreshTokens" (
  "RefreshTokenId" serial PRIMARY KEY,
  "UserId" integer NOT NULL,
  "Token" text NOT NULL,
  "ExpiryDate" timestamp with time zone NOT NULL,
  "IsRevoked" boolean NOT NULL,
  CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

CREATE TABLE "Conclusions" (
  "ConclusionId" serial PRIMARY KEY,
  "MeetingId" integer NOT NULL,
  "Content" text NOT NULL,
  CONSTRAINT "FK_Conclusions_Meetings_MeetingId" FOREIGN KEY ("MeetingId") REFERENCES "Meetings" ("MeetingId") ON DELETE CASCADE
);

CREATE TABLE "Documents" (
  "DocumentId" serial PRIMARY KEY,
  "MeetingId" integer NOT NULL,
  "DocumentType" text NOT NULL,
  "FileName" varchar(255) NOT NULL,
  "FilePath" text NOT NULL,
  "UploadedAt" timestamp with time zone NOT NULL,
  CONSTRAINT "FK_Documents_Meetings_MeetingId" FOREIGN KEY ("MeetingId") REFERENCES "Meetings" ("MeetingId") ON DELETE CASCADE
);

CREATE TABLE "MeetingAudios" (
  "MeetingAudioId" serial PRIMARY KEY,
  "MeetingId" integer NOT NULL,
  "AudioFileName" varchar(255) NOT NULL,
  "AudioFilePath" text NOT NULL,
  "RecordedAt" timestamp with time zone NOT NULL,
  CONSTRAINT "FK_MeetingAudios_Meetings_MeetingId" FOREIGN KEY ("MeetingId") REFERENCES "Meetings" ("MeetingId") ON DELETE CASCADE
);

CREATE TABLE "MeetingLogs" (
  "MeetingLogId" serial PRIMARY KEY,
  "MeetingId" integer NOT NULL,
  "Action" varchar(255) NOT NULL,
  "Timestamp" timestamp with time zone NOT NULL,
  "Details" text,
  CONSTRAINT "FK_MeetingLogs_Meetings_MeetingId" FOREIGN KEY ("MeetingId") REFERENCES "Meetings" ("MeetingId") ON DELETE CASCADE
);

CREATE TABLE "MeetingParticipants" (
  "MeetingId" integer NOT NULL,
  "UserId" integer NOT NULL,
  "ParticipationStatus" text NOT NULL,
  "JoinedAt" timestamp with time zone,
  "LeftAt" timestamp with time zone,
  PRIMARY KEY ("MeetingId","UserId"),
  CONSTRAINT "FK_MeetingParticipants_Meetings_MeetingId" FOREIGN KEY ("MeetingId") REFERENCES "Meetings" ("MeetingId") ON DELETE CASCADE,
  CONSTRAINT "FK_MeetingParticipants_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

CREATE TABLE "Notes" (
  "NoteId" serial PRIMARY KEY,
  "MeetingId" integer NOT NULL,
  "UserId" integer NOT NULL,
  "Content" text NOT NULL,
  "CreatedAt" timestamp with time zone NOT NULL,
  CONSTRAINT "FK_Notes_Meetings_MeetingId" FOREIGN KEY ("MeetingId") REFERENCES "Meetings" ("MeetingId") ON DELETE CASCADE,
  CONSTRAINT "FK_Notes_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

CREATE TABLE "Polls" (
  "PollId" serial PRIMARY KEY,
  "MeetingId" integer NOT NULL,
  "Question" varchar(500) NOT NULL,
  "CreatedAt" timestamp with time zone NOT NULL,
  CONSTRAINT "FK_Polls_Meetings_MeetingId" FOREIGN KEY ("MeetingId") REFERENCES "Meetings" ("MeetingId") ON DELETE CASCADE
);

CREATE TABLE "UserVotes" (
  "UserVoteId" serial PRIMARY KEY,
  "UserId" integer NOT NULL,
  "PollId" integer NOT NULL,
  "VoteValue" varchar(50) NOT NULL,
  "VotedAt" timestamp with time zone NOT NULL,
  CONSTRAINT "FK_UserVotes_Polls_PollId" FOREIGN KEY ("PollId") REFERENCES "Polls" ("PollId") ON DELETE CASCADE,
  CONSTRAINT "FK_UserVotes_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

-- Seed departments
INSERT INTO "Departments" ("DepartmentId","DepartmentName") VALUES
(1,'Phòng Bảo vệ'),
(2,'Phòng Kỹ thuật'),
(3,'Phòng Kế toán'),
(4,'Ban Giám đốc'),
(5,'Phòng Nhân sự');

COMMIT;
