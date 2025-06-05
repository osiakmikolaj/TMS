using Grpc.Net.Client;
using TMS.GRPC;

namespace TMS.RazorPages.Services
{
    public class GrpcTicketClient
    {
        private readonly TicketService.TicketServiceClient _client;

        public GrpcTicketClient(IConfiguration configuration)
        {
            var channel = GrpcChannel.ForAddress(configuration["GrpcSettings:TicketServiceUrl"]);
            _client = new TicketService.TicketServiceClient(channel);
        }

        public async Task<GetTicketsResponse> GetTicketsAsync()
        {
            return await _client.GetTicketsAsync(new GetTicketsRequest());
        }

        public async Task<GetTicketResponse> GetTicketAsync(int id)
        {
            return await _client.GetTicketAsync(new GetTicketRequest { Id = id });
        }

        public async Task<CreateTicketResponse> CreateTicketAsync(CreateTicketRequest request)
        {
            return await _client.CreateTicketAsync(request);
        }

        public async Task<UpdateTicketResponse> UpdateTicketAsync(UpdateTicketRequest request)
        {
            return await _client.UpdateTicketAsync(request);
        }

        public async Task<DeleteTicketResponse> DeleteTicketAsync(int id)
        {
            return await _client.DeleteTicketAsync(new DeleteTicketRequest { Id = id });
        }
    }
}