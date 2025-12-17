using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Services;
using CentralizedSalesSystem.API.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CentralizedSalesDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}
);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IItemVariationService, ItemVariationService>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IServiceChargeService, ServiceChargeService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOwnerSignupService, OwnerSignupService>();
builder.Services.AddScoped<IRefundService, RefundService>();
builder.Services.AddScoped<IGiftCardService, GiftCardService>();
builder.Services.AddSingleton<DbSeeder>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();


var jwtSection = builder.Configuration.GetSection("JWT");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("JWT: Key is missing in config");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = JwtRegisteredClaimNames.Sub,

            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });
    
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("RequireManageAll", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(c =>
                (c.Type == PermissionAuthorizationHandler.PermissionClaimType
                 || c.Type == PermissionAuthorizationHandler.LegacyPermissionClaimType)
                && string.Equals(c.Value, PermissionCode.MANAGE_ALL.ToString(), StringComparison.OrdinalIgnoreCase))));
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearerAuth"
                },
                Scheme = "bearer",
                Name = "Authorization",
                In = ParameterLocation.Header
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWasmClient", policy =>
    {
        policy
            // Allow both dev HTTPS ports (API and WASM host) to call the API.
            .WithOrigins(
                "https://localhost:5001",
                "https://localhost:7054",
                "https://localhost:7051")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var dbSeeder = app.Services.GetRequiredService<DbSeeder>();
// Always ensure SuperAdmins exist (idempotent)
await dbSeeder.SeedSuperAdminsAsync();
// Full sample seed only in development
if (app.Environment.IsDevelopment())
{
    await dbSeeder.SeedAsync();
}

app.UseHttpsRedirection();


app.UseCors("AllowWasmClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
