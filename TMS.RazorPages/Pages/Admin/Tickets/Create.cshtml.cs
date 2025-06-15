using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TMS.Domain.Entities;
using TMS.Infrastructure.Persistence;

namespace TMS.RazorPages.Pages.Admin.Tickets
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SelectList Projects { get; set; }
        public SelectList Users { get; set; }
        public SelectList Statuses { get; set; }

        [BindProperty]
        public Ticket Ticket { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
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

            var username = User.Identity.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (currentUser == null)
            {
                ModelState.AddModelError(string.Empty, "Nie mo¿na znaleŸæ aktualnego u¿ytkownika.");
                await LoadSelectListsAsync();
                return Page();
            }

            Ticket.CreatedById = currentUser.Id;
            Ticket.CreatedAt = DateTime.Now;

            _context.Tickets.Add(Ticket);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task LoadSelectListsAsync()
        {
            Projects = new SelectList(await _context.Projects.ToListAsync(), "Id", "Name");
            Users = new SelectList(await _context.Users.ToListAsync(), "Id", "Username");
            Statuses = new SelectList(Enum.GetNames(typeof(TicketStatus)));
        }
    }
}