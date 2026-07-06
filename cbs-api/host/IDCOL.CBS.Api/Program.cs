using System.Text;
using FluentValidation;
using IDCOL.CBS.Api.Security;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.BuildingBlocks.Application.Behaviors;
using IDCOL.CBS.SystemAdmin.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Application.Users.Commands.CreateUser;
using IDCOL.CBS.SystemAdmin.Domain.Entities;
using IDCOL.CBS.SystemAdmin.Infrastructure;
using IDCOL.CBS.SystemAdmin.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSystemAdminInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();

// Every bounded context's Application assembly is registered here as the module set grows -
// for Phase 0 that is just SystemAdmin.
var applicationAssembly = typeof(CreateUserCommand).Assembly;
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
builder.Services.AddValidatorsFromAssembly(applicationAssembly);

// Pipeline order = outer-to-inner: validate input, then check maker-checker authorization,
// then run the handler + commit its transaction, THEN write the audit row (so the log reflects
// an already-durable state change).
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MakerCheckerGateBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey configuration is required.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Database:UseSqliteForLocalDevelopment"))
{
    await EnsureLocalDevDatabaseAsync(app.Services, builder.Configuration);
}

app.UseCors("AngularDev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestampUtc = DateTime.UtcNow }));

app.Run();

// Dev-only convenience so the app is runnable without provisioning a real Oracle instance
// first: creates the SQLite schema from the current EF model (no migration needed - SQLite is
// never the real target, so it doesn't get its own migration set) and seeds exactly one admin
// user if the Users table is empty. Guarded by both IsDevelopment() and the explicit
// Database:UseSqliteForLocalDevelopment flag at the call site, so this never runs against Oracle.
static async Task EnsureLocalDevDatabaseAsync(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SystemAdminDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    if (await dbContext.Users.AnyAsync())
    {
        return;
    }

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var username = configuration["DevSeed:AdminUsername"] ?? "admin";
    var plainTextPassword = configuration["DevSeed:AdminPassword"] ?? "Admin@123456";

    var admin = User.Create(
        Guid.NewGuid(),
        username,
        "Local Dev Admin",
        "dev-admin@idcol.local",
        passwordHasher.Hash(plainTextPassword),
        "IT",
        "dev-seed");

    dbContext.Users.Add(admin);
    await dbContext.SaveChangesAsync();

    Log.Information(
        "Dev-only SQLite fallback active - seeded admin user {Username}. Never enable Database:UseSqliteForLocalDevelopment outside local development.",
        username);
}

// Exposed so WebApplicationFactory<Program> can be used by API integration tests.
public partial class Program
{
}
