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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Ticket Ticket { get; set; }

        public SelectList Projects { get; set; }
        public SelectList Users { get; set; }
        public SelectList Statuses { get; set; }
        public string CreatedByUsername { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket = await _context.Tickets
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (Ticket == null)
            {
                return NotFound();
            }

            CreatedByUsername = Ticket.CreatedBy?.Username;
            await LoadSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            // Ustaw datê aktualizacji
            Ticket.UpdatedAt = DateTime.Now;

            _context.Attach(Ticket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(Ticket.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadSelectListsAsync()
        {
            Projects = new SelectList(await _context.Projects.ToListAsync(), "Id", "Name");
            Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Username");
            Statuses = new SelectList(Enum.GetNames(typeof(TicketStatus)));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}