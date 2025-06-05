using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TMS.Infrastructure.Persistence;

namespace TMS.RazorPages.Pages.Admin
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int ProjectCount { get; set; }
        public int UserCount { get; set; }
        public int TicketCount { get; set; }

        public async Task OnGetAsync()
        {
            ProjectCount = await _context.Projects.CountAsync();
            UserCount = await _context.Users.CountAsync();
            TicketCount = await _context.Tickets.CountAsync();
        }
    }
}