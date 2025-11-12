ALTER TABLE "Users" ALTER COLUMN "Role" TYPE integer USING (CASE WHEN "Role" ~ '^[0-9]+$' THEN "Role"::integer ELSE 0 END);
