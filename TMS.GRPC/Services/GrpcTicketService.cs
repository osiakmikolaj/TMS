using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;
using TMS.Application.Common.Interfaces;
using TMS.Domain.Entities;
using TMS.GRPC;

namespace TMS.GRPC.Services
{
    // Dziedziczenie z TMS.GRPC.TicketService.TicketServiceBase jest kluczowe
    public class GrpcTicketService : TMS.GRPC.TicketService.TicketServiceBase
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<GrpcTicketService> _logger;

        public GrpcTicketService(IApplicationDbContext context, ILogger<GrpcTicketService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<GetTicketsResponse> GetTickets(GetTicketsRequest request, ServerCallContext serverCallContext)
        {
            _logger.LogInformation("GRPC GetTickets request received. ProjectId: {ProjectId}", request.HasProjectId ? request.ProjectId.ToString() : "Any");
            var query = _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .AsQueryable();

            if (request.HasProjectId) // Dla optional int32, sprawdzamy obecność wartości
            {
                query = query.Where(t => t.ProjectId == request.ProjectId);
            }

            var tickets = await query.ToListAsync(serverCallContext.CancellationToken);

            var response = new GetTicketsResponse();
            response.Tickets.AddRange(tickets.Select(MapToTicketResponse));
            _logger.LogInformation("GRPC GetTickets returned {Count} tickets.", response.Tickets.Count);
            return response;
        }

        public override async Task<TicketResponse> GetTicket(GetTicketRequest request, ServerCallContext serverCallContext)
        {
            _logger.LogInformation("GRPC GetTicket request received for ID: {TicketId}", request.Id);
            var ticket = await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == request.Id, serverCallContext.CancellationToken);

            if (ticket == null)
            {
                _logger.LogWarning("GRPC GetTicket: Ticket with ID {TicketId} not found.", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, $"Ticket with ID {request.Id} not found"));
            }
            _logger.LogInformation("GRPC GetTicket: Ticket with ID {TicketId} found.", request.Id);
            return MapToTicketResponse(ticket);
        }

        public override async Task<TicketResponse> CreateTicket(CreateTicketRequest request, ServerCallContext serverCallContext)
        {
            _logger.LogInformation("GRPC CreateTicket request received for Title: {Title}", request.Title);
            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                Status = TicketStatus.New, // Domyślny status dla nowego zgłoszenia
                CreatedById = request.CreatedById,
                AssignedToId = request.HasAssignedToId ? request.AssignedToId : (int?)null,
                ProjectId = request.ProjectId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync(serverCallContext.CancellationToken);
            _logger.LogInformation("GRPC CreateTicket: Ticket created with ID {TicketId}", ticket.Id);

            // Ponowne załadowanie zgłoszenia z właściwościami nawigacyjnymi
            var createdTicket = await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .FirstAsync(t => t.Id == ticket.Id, serverCallContext.CancellationToken);

            return MapToTicketResponse(createdTicket);
        }

        public override async Task<TicketResponse> UpdateTicket(UpdateTicketRequest request, ServerCallContext serverCallContext)
        {
            _logger.LogInformation("GRPC UpdateTicket request received for ID: {TicketId}", request.Id);
            var ticket = await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == request.Id, serverCallContext.CancellationToken);

            if (ticket == null)
            {
                _logger.LogWarning("GRPC UpdateTicket: Ticket with ID {TicketId} not found.", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, $"Ticket with ID {request.Id} not found"));
            }

            if (request.HasTitle)
                ticket.Title = request.Title;
            if (request.HasDescription)
                ticket.Description = request.Description;
            if (request.HasStatus)
                ticket.Status = (TicketStatus)request.Status;

            // Poprawiona obsługa pola ClearAssignedToId
            // Sprawdź czy pole jest obecne w żądaniu
            if (request.HasClearAssignedToId)
            {
                // Pola opcjonalnego bool nie możemy sprawdzić bezpośrednio przez 
                // `request.ClearAssignedToId` bo to jest metoda, nie właściwość.
                // Po prostu ustawiamy AssignedToId na null, gdy pole jest obecne.
                ticket.AssignedToId = null;
            }
            else if (request.HasAssignedToId)
            {
                ticket.AssignedToId = request.AssignedToId;
            }

            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(serverCallContext.CancellationToken);
            _logger.LogInformation("GRPC UpdateTicket: Ticket with ID {TicketId} updated.", ticket.Id);

            return MapToTicketResponse(ticket);
        }

        public override async Task<DeleteTicketResponse> DeleteTicket(DeleteTicketRequest request, ServerCallContext serverCallContext)
        {
            _logger.LogInformation("GRPC DeleteTicket request received for ID: {TicketId}", request.Id);
            var ticket = await _context.Tickets.FindAsync(new object[] { request.Id }, serverCallContext.CancellationToken);

            if (ticket == null)
            {
                _logger.LogWarning("GRPC DeleteTicket: Ticket with ID {TicketId} not found.", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, $"Ticket with ID {request.Id} not found"));
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync(serverCallContext.CancellationToken);
            _logger.LogInformation("GRPC DeleteTicket: Ticket with ID {TicketId} deleted.", request.Id);

            return new DeleteTicketResponse { Success = true };
        }

        private static TicketResponse MapToTicketResponse(Ticket ticket)
        {
            var createdByName = ticket.CreatedBy != null ? $"{ticket.CreatedBy.FirstName} {ticket.CreatedBy.LastName}" : "N/A";
            var projectName = ticket.Project != null ? ticket.Project.Name : "N/A";
            string? assignedToName = null;
            if (ticket.AssignedToId.HasValue && ticket.AssignedTo != null)
            {
                assignedToName = $"{ticket.AssignedTo.FirstName} {ticket.AssignedTo.LastName}";
            }

            var response = new TicketResponse
            {
                Id = ticket.Id,
                Title = ticket.Title ?? "",
                Description = ticket.Description ?? "",
                Status = (int)ticket.Status,
                CreatedById = ticket.CreatedById,
                CreatedByName = createdByName,
                ProjectId = ticket.ProjectId,
                ProjectName = projectName,
                CreatedAt = ticket.CreatedAt.ToString("o") // Format ISO 8601
            };

            // Ustawianie pól opcjonalnych tylko jeśli mają wartość
            if (ticket.AssignedToId.HasValue)
            {
                response.AssignedToId = ticket.AssignedToId.Value;
            }
            if (assignedToName != null)
            {
                response.AssignedToName = assignedToName;
            }
            if (ticket.UpdatedAt.HasValue)
            {
                response.UpdatedAt = ticket.UpdatedAt.Value.ToString("o");
            }

            return response;
        }
    }
}