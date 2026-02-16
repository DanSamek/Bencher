## WebApplication

### Requirements
- `.net 9.0+ (+runtime)`
- `postgres 15.1`
- `Docker + Docker compose (or PostgreSQL manually configured)`
- `WSL (for windows)`
- `dotnet tool ef (command to install: dotnet tool install --global dotnet-ef)`

### Environment variables 

Template for usage is ready in the file `.empty.env`.
Before run rename it to `.env`

```
Database connection:
DB_HOST, DB_NAME, DB_USER, DB_PASS

Admin account setup:
ADMIN_USERNAME, ADMIN_PASSWORD, ADMIN_EMAIL

Forgot password setup:
GMAIL_EMAIL, GMAIL_PASSWORD (see in the gmail: "App passwords"), GMAIL_SUBJECT (optional - default is "Bencher - reset password")
```

All environment variables are required for the run except `GMAIL_SUBJECT`.

### Database setup (locally)
- `cd WebApplication`
- `docker compose -f postgres-compose.yml up`
- `dotnet ef database update` (only when first run or new migration)

*NOTE: This will start the database with configuration from `.env` (It will be available on localhost:5432 so this port has to be free!)*

### How to run (locally)
- `cd WebApplication`
- `dotnet run -c Release`


**See also manual.md**