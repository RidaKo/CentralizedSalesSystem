# CentralSalesSystem
## Database Setup and Management

### Initial Setup using Docker

1. Install Docker on your system and run it

These commands should be run in the .API directory:

2. Start the containers needed for the database:
```bash
docker-compose up -d 
```

3. To reset the database fully:
```bash
docker-compose down -v 
```
Then start the containers again using the command from step 1.

### Install Required Tools

Install the Entity Framework Core CLI tools:
```bash
dotnet tool install --global dotnet-ef
```

Note: All subsequent commands should be run in the .API directory, otherwise you'll have to specify the project manually.

### Database Migrations

#### Apply Existing Migrations

After setting up the database containers, apply existing migrations to create the database schema:
```bash
dotnet ef database update
```

#### Create New Database Tables

1. Make changes to your model classes in the C# code

2. Add your table to CentralizedSalesDbContext.cs:
```csharp
public DbSet<YourModel> Models { get; set; }
```


3. Create a new migration:
```bash
dotnet ef migrations add YourMigrationName
```
Replace `YourMigrationName` with a descriptive name for your changes (e.g., `AddUserProfileTable`)

4. Apply the new migration:
```bash
dotnet ef database update
```

#### Common Migration Commands

- List all migrations:
```bash
dotnet ef migrations list
```

- Remove the last migration (if not yet applied to the database):
```bash
dotnet ef migrations remove
```