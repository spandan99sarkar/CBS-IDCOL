using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using IDCOL.CBS.Api.Infrastructure;
using IDCOL.CBS.Api.Security;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.BuildingBlocks.Application.Behaviors;
using IDCOL.CBS.LoanLifecycle.Infrastructure;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
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

// Enums serialize/bind as their string name (e.g. "Reschedule"), not the numeric ordinal - the
// Angular clients send/expect string literals for every enum-typed field (FacilityVersionEventType
// etc.), and numeric ordinals would also be a silent foot-gun if the enum's member order ever
// changes.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSystemAdminInfrastructure(builder.Configuration);
builder.Services.AddLoanLifecycleInfrastructure(builder.Configuration);

// One commit point across every module's DbContext (see CompositeUnitOfWork). Registered after
// the module infrastructure so it overrides the single-context UnitOfWork from SystemAdmin.
builder.Services.AddScoped<IUnitOfWork, CompositeUnitOfWork>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();

// Every bounded context's Application assembly is registered here as the module set grows.
var systemAdminAssembly = typeof(CreateUserCommand).Assembly;
var repaymentEngineAssembly = typeof(IDCOL.CBS.RepaymentEngine.Application.ComputeSchedule.ComputeScheduleQuery).Assembly;
var productConfigAssembly = typeof(IDCOL.CBS.ProductConfig.Application.Products.CreateProductCommand).Assembly;
var partyKycAssembly = typeof(IDCOL.CBS.PartyKyc.Application.Customers.CreateCustomerCommand).Assembly;
var creditSanctionAssembly = typeof(IDCOL.CBS.CreditSanction.Application.Sanctions.CreateSanctionCommand).Assembly;
var disbursementAssembly = typeof(IDCOL.CBS.Disbursement.Application.Commands.InitiateDisbursementCommand).Assembly;
var collectionAssembly = typeof(IDCOL.CBS.Collection.Application.Commands.EnterReceiptCommand).Assembly;
var classificationAssembly = typeof(IDCOL.CBS.Classification.Application.Commands.RunClassificationCommand).Assembly;
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(systemAdminAssembly);
    cfg.RegisterServicesFromAssembly(repaymentEngineAssembly);
    cfg.RegisterServicesFromAssembly(productConfigAssembly);
    cfg.RegisterServicesFromAssembly(partyKycAssembly);
    cfg.RegisterServicesFromAssembly(creditSanctionAssembly);
    cfg.RegisterServicesFromAssembly(disbursementAssembly);
    cfg.RegisterServicesFromAssembly(collectionAssembly);
    cfg.RegisterServicesFromAssembly(classificationAssembly);
});
builder.Services.AddValidatorsFromAssembly(systemAdminAssembly);
builder.Services.AddValidatorsFromAssembly(repaymentEngineAssembly);
builder.Services.AddValidatorsFromAssembly(productConfigAssembly);
builder.Services.AddValidatorsFromAssembly(partyKycAssembly);
builder.Services.AddValidatorsFromAssembly(creditSanctionAssembly);
builder.Services.AddValidatorsFromAssembly(disbursementAssembly);
builder.Services.AddValidatorsFromAssembly(collectionAssembly);

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

    // Each module DbContext owns its own dev SQLite file, so EnsureCreated works per file.
    var lifecycleDb = scope.ServiceProvider.GetRequiredService<LoanLifecycleDbContext>();
    await lifecycleDb.Database.EnsureCreatedAsync();

    // Seed the DFIM 04/2021 classification threshold matrix + provisioning rates (config-driven).
    if (!await lifecycleDb.ClassificationThresholds.AnyAsync())
    {
        lifecycleDb.ClassificationThresholds.AddRange(IDCOL.CBS.Classification.Domain.DfimSeed.Thresholds());
        lifecycleDb.ProvisioningRates.AddRange(IDCOL.CBS.Classification.Domain.DfimSeed.Rates());
        await lifecycleDb.SaveChangesAsync();
    }

    // Seed all 19 real IDCOL borrowers' historical schedules (original + every reschedule/
    // restructure/prepayment event) as live Customer/Sanction/Facility/FacilityVersion data.
    await IDCOL.CBS.Api.Infrastructure.BorrowerHistorySeed.SeedAsync(lifecycleDb);

    // Seed downstream operational activity (disbursements, collections, classification run) on
    // top of that borrower history so the Disbursement/Collection/Classification screens and the
    // CAD/F&A reports have coherent, schedule-derived data instead of being empty.
    await IDCOL.CBS.Api.Infrastructure.LifecycleActivitySeed.SeedAsync(lifecycleDb);

    // Seed the Loan Security & Covenant register (collateral instruments + covenant obligations)
    // so the security dashboard and its expiry/recommended-action engine have data.
    await IDCOL.CBS.Api.Infrastructure.SecurityCovenantSeed.SeedAsync(lifecycleDb);

    if (await dbContext.Users.AnyAsync())
    {
        return;
    }

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var username = configuration["DevSeed:AdminUsername"] ?? "admin";
    var plainTextPassword = configuration["DevSeed:AdminPassword"] ?? "Admin@123456";
    var hash = passwordHasher.Hash(plainTextPassword);

    // Department roles used both for nav gating and for the disbursement workflow's stage checks.
    var roles = new[]
    {
        Role.Create(Guid.NewGuid(), "ADMIN", "Administrator", "Full access", "dev-seed"),
        Role.Create(Guid.NewGuid(), "BU", "Business Unit", "Initiates disbursements", "dev-seed"),
        Role.Create(Guid.NewGuid(), "CAD", "Credit Administration", "Reviews disbursements", "dev-seed"),
        Role.Create(Guid.NewGuid(), "ACCOUNTS", "Accounts", "Posts disbursements to GL", "dev-seed"),
    };
    dbContext.Roles.AddRange(roles);

    Role RoleByCode(string code) => roles.First(r => r.Code == code);

    // admin holds every role for general use; the domain still blocks a single user from
    // performing more than one stage of the same disbursement, so the 3-stage flow needs the
    // three department users below to be completed end to end.
    var admin = User.Create(Guid.NewGuid(), username, "Local Dev Admin", "dev-admin@idcol.local", hash, "IT", "dev-seed");
    foreach (var r in roles) admin.AssignToRole(r);

    var buUser = User.Create(Guid.NewGuid(), "bu1", "BU Officer", "bu1@idcol.local", hash, "IF", "dev-seed");
    buUser.AssignToRole(RoleByCode("BU"));

    var cadUser = User.Create(Guid.NewGuid(), "cad1", "CAD Officer", "cad1@idcol.local", hash, "CAD", "dev-seed");
    cadUser.AssignToRole(RoleByCode("CAD"));

    var accountsUser = User.Create(Guid.NewGuid(), "acct1", "Accounts Officer", "acct1@idcol.local", hash, "ACCOUNTS", "dev-seed");
    accountsUser.AssignToRole(RoleByCode("ACCOUNTS"));

    dbContext.Users.AddRange(admin, buUser, cadUser, accountsUser);
    await dbContext.SaveChangesAsync();

    Log.Information(
        "Dev-only SQLite fallback active - seeded users admin/bu1/cad1/acct1 (password {Password}). Never enable Database:UseSqliteForLocalDevelopment outside local development.",
        plainTextPassword);
}

// Exposed so WebApplicationFactory<Program> can be used by API integration tests.
public partial class Program
{
}
