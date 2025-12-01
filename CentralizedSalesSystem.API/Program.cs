using Microsoft.EntityFrameworkCore;
using CentralizedSalesSystem.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Console.WriteLine($"Using connection string: {builder.Configuration.GetConnectionString("DefaultConnection")}");

builder.Services.AddDbContext<CentralizedSalesDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.MapGet("/test-db", async (CentralizedSalesDbContext db) =>
{
    var count = await db.Users.CountAsync();
    return Results.Ok(new { Message = "DB Connected!", Count = count });
});

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Using connection string: {connection}");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
