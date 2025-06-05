using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TMS.Domain.Entities;
using TMS.Infrastructure.Persistence;

namespace TMS.RazorPages.Pages.Admin.Users
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public User User { get; set; }
        public List<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
        public List<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (User == null)
            {
                return NotFound();
            }

            AssignedTickets = await _context.Tickets
                .Include(t => t.Project)
                .Where(t => t.AssignedToId == id)
                .ToListAsync();

            CreatedTickets = await _context.Tickets
                .Include(t => t.Project)
                .Where(t => t.CreatedById == id)
                .ToListAsync();

            return Page();
        }
    }
}