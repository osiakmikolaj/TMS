using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMS.Domain.Entities;
using TMS.Infrastructure.Persistence;

namespace TMS.RazorPages.Pages.Admin.Tickets
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Ticket> Tickets { get; set; } = new List<Ticket>();
        public SelectList ProjectList { get; set; }
        public SelectList UserList { get; set; }
        public SelectList StatusList { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ProjectId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? AssignedToId { get; set; }

        public async Task OnGetAsync()
        {
            // Budowanie zapytania z uwzglêdnieniem filtrów
            var ticketsQuery = _context.Tickets
                .Include(t => t.Project)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            if (ProjectId.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.ProjectId == ProjectId.Value);
            }

            if (!string.IsNullOrEmpty(Status) && Enum.TryParse<TicketStatus>(Status, out var statusEnum))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Status == statusEnum);
            }

            if (AssignedToId.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.AssignedToId == AssignedToId.Value);
            }

            Tickets = await ticketsQuery.OrderByDescending(t => t.CreatedAt).ToListAsync();

            // Przygotowanie list dla filtrów
            ProjectList = new SelectList(await _context.Projects.ToListAsync(), "Id", "Name");
            UserList = new SelectList(await _context.Users.ToListAsync(), "Id", "Username");
            StatusList = new SelectList(Enum.GetNames(typeof(TicketStatus)));
        }
    }
}