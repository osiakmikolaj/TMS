using TMS.Application;
using TMS.GRPC.Services;
using TMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Dodaj warstwy aplikacji i infrastruktury
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Dodaj us³ugi gRPC
builder.Services.AddGrpc();
// Opcjonalnie: dodaj refleksjê dla narzêdzi takich jak grpcurl
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

var app = builder.Build();

// Skonfiguruj pipeline HTTP
app.UseRouting();

// Mapowanie us³ug gRPC
app.MapGrpcService<GrpcTicketService>();
app.MapGrpcService<GreeterService>();

// Opcjonalnie: mapuj refleksjê gRPC
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "TMS gRPC Service is running. Communicate via a gRPC client.");

app.Run();