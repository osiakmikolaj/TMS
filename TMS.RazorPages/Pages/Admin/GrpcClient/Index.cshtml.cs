using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TMS.RazorPages.Services;

namespace TMS.RazorPages.Pages.Admin.GrpcClient
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class IndexModel : PageModel
    {
        private readonly GrpcTicketClient _grpcClient;

        public IndexModel(GrpcTicketClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        public List<TicketDto> Tickets { get; set; } = new List<TicketDto>();

        public async Task OnGetAsync()
        {
            var response = await _grpcClient.GetTicketsAsync();

            foreach (var ticket in response.Tickets)
            {
                Tickets.Add(new TicketDto
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Status = ticket.Status,
                    ProjectId = ticket.ProjectId,
                    ProjectName = ticket.ProjectName
                });
            }
        }
    }

    public class TicketDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
}