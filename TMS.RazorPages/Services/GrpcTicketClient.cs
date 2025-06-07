using Grpc.Net.Client;
using TMS.GRPC;

namespace TMS.RazorPages.Services
{
    public class GrpcTicketClient
    {
        private readonly TicketService.TicketServiceClient _client;

        public GrpcTicketClient(IConfiguration configuration)
        {
            var grpcServiceUrl = configuration["GrpcSettings:TicketServiceUrl"];
            if (string.IsNullOrEmpty(grpcServiceUrl))
            {
                throw new ArgumentNullException(nameof(grpcServiceUrl), "GrpcSettings:TicketServiceUrl is not configured.");
            }
            var channel = GrpcChannel.ForAddress(grpcServiceUrl);
            _client = new TicketService.TicketServiceClient(channel);
        }

        public async Task<GetTicketsResponse> GetTicketsAsync()
        {
            return await _client.GetTicketsAsync(new GetTicketsRequest());
        }

        public async Task<TicketResponse> GetTicketAsync(int id)
        {
            return await _client.GetTicketAsync(new GetTicketRequest { Id = id });
        }

        public async Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request)
        {
            return await _client.CreateTicketAsync(request);
        }

        public async Task<TicketResponse> UpdateTicketAsync(UpdateTicketRequest request)
        {
            return await _client.UpdateTicketAsync(request);
        }

        public async Task<DeleteTicketResponse> DeleteTicketAsync(int id)
        {
            return await _client.DeleteTicketAsync(new DeleteTicketRequest { Id = id });
        }
    }
}