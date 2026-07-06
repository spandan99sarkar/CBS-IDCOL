using IDCOL.CBS.SystemAdmin.Infrastructure;
using IDCOL.CBS.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSystemAdminInfrastructure(builder.Configuration);
builder.Services.AddHostedService<HeartbeatBackgroundService>();

var host = builder.Build();
host.Run();
