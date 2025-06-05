using System.Xml.Linq;

namespace TMS.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

public enum TicketStatus
{
    New,
    InProgress,
    Testing,
    Completed,
    Closed
}