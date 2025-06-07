using TMS.Application;
using TMS.GRPC.Services;
using TMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddGrpc();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

var app = builder.Build();

app.UseRouting();

app.MapGrpcService<GrpcTicketService>();
app.MapGrpcService<GreeterService>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "TMS gRPC Service is running. Communicate via a gRPC client.");

app.Run();