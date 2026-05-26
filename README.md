# indor

ASP.NET Core MVC app for Indor — property management, services, microservices scheduling, and owner dashboard.

## Setup

1. Copy `IndorMvcApp/appsettings.Example.json` to `IndorMvcApp/appsettings.json` and set your SQL Server connection string.
2. Run the SQL scripts in `IndorMvcApp/Scripts/` (see `README-DATABASE.md`).
3. From `IndorMvcApp/`:

```bash
dotnet run
```

Default URL: `http://localhost:5268`
