using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using TMS.Application.Common.Interfaces;
using TMS.Domain.Entities;

namespace TMS.GRPC.Services;

public class TicketService : GRPC.TicketService.TicketServiceBase
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IApplicationDbContext context, ILogger<TicketService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task<GetTicketsResponse> GetTickets(GetTicketsRequest request, ServerCallContext context)
    {
        var query = _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .AsQueryable();

        if (request.ProjectId != null)
        {
            query = query.Where(t => t.ProjectId == request.ProjectId);
        }

        var tickets = await query.ToListAsync();

        var response = new GetTicketsResponse();
        response.Tickets.AddRange(tickets.Select(MapToTicketResponse));

        return response;
    }

    public override async Task<TicketResponse> GetTicket(GetTicketRequest request, ServerCallContext context)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == request.Id);

        if (ticket == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Ticket with ID {request.Id} not found"));
        }

        return MapToTicketResponse(ticket);
    }

    public override async Task<TicketResponse> CreateTicket(CreateTicketRequest request, ServerCallContext context)
    {
        var ticket = new Ticket
        {
            Title = request.Title,
            Description = request.Description,
            Status = TicketStatus.New,
            CreatedById = request.CreatedById,
            AssignedToId = request.AssignedToId,
            ProjectId = request.ProjectId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Reload the ticket with navigation properties
        ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .FirstAsync(t => t.Id == ticket.Id);

        return MapToTicketResponse(ticket);
    }

    public override async Task<TicketResponse> UpdateTicket(UpdateTicketRequest request, ServerCallContext context)
    {
        var ticket = await _context.Tickets.FindAsync(request.Id);

        if (ticket == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Ticket with ID {request.Id} not found"));
        }

        if (request.Title != null)
            ticket.Title = request.Title;

        if (request.Description != null)
            ticket.Description = request.Description;

        if (request.Status != null)
            ticket.Status = (TicketStatus)request.Status;

        if (request.AssignedToId != null)
            ticket.AssignedToId = request.AssignedToId;

        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(CancellationToken.None);

        // Reload the ticket with navigation properties
        ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .FirstAsync(t => t.Id == ticket.Id);

        return MapToTicketResponse(ticket);
    }

    public override async Task<DeleteTicketResponse> DeleteTicket(DeleteTicketRequest request, ServerCallContext context)
    {
        var ticket = await _context.Tickets.FindAsync(request.Id);

        if (ticket == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Ticket with ID {request.Id} not found"));
        }

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync(CancellationToken.None);

        return new DeleteTicketResponse { Success = true };
    }

    private static TicketResponse MapToTicketResponse(Ticket ticket)
    {
        return new TicketResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = (int)ticket.Status,
            CreatedById = ticket.CreatedById,
            CreatedByName = $"{ticket.CreatedBy.FirstName} {ticket.CreatedBy.LastName}",
            ProjectId = ticket.ProjectId,
            ProjectName = ticket.Project.Name,
            CreatedAt = ticket.CreatedAt.ToString("o")
        };
    }
}