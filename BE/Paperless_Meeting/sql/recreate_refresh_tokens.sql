BEGIN;
DROP TABLE IF EXISTS "RefreshTokens";

CREATE TABLE "RefreshTokens" (
  "Id" serial PRIMARY KEY,
  "Token" text NOT NULL,
  "UserId" integer NOT NULL,
  "CreatedAt" timestamp with time zone NOT NULL,
  "ExpiresAt" timestamp with time zone NOT NULL,
  "IsRevoked" boolean NOT NULL,
  CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

COMMIT;
