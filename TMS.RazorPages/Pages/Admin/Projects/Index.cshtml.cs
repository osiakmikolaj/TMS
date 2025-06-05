using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TMS.Domain.Entities;
using TMS.Infrastructure.Persistence;

namespace TMS.RazorPages.Pages.Admin.Projects
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Project> Projects { get; set; } = new List<Project>();

        public async Task OnGetAsync()
        {
            Projects = await _context.Projects.ToListAsync();
        }
    }
}