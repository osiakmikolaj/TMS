using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.Application.Common.Interfaces;
using TMS.Domain.Entities;

namespace TMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<TicketController> _logger;

        public TicketController(IApplicationDbContext context, ILogger<TicketController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Ticket
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            return await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .ToListAsync();
        }

        // GET: api/Ticket/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return ticket;
        }

        // POST: api/Ticket
        [HttpPost]
        public async Task<ActionResult<Ticket>> CreateTicket(Ticket ticket)
        {
            ticket.CreatedAt = DateTime.UtcNow;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync(CancellationToken.None);

            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
        }

        // PUT: api/Ticket/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, Ticket ticketUpdate)
        {
            if (id != ticketUpdate.Id)
            {
                return BadRequest();
            }

            // Najpierw pobierz istniejący bilet
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Aktualizuj właściwości pojedynczo
            ticket.Title = ticketUpdate.Title;
            ticket.Description = ticketUpdate.Description;
            ticket.Status = ticketUpdate.Status;
            ticket.AssignedToId = ticketUpdate.AssignedToId;
            ticket.ProjectId = ticketUpdate.ProjectId;
            ticket.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync(CancellationToken.None);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Ticket/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync(CancellationToken.None);

            return NoContent();
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}