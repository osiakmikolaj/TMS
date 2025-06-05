﻿namespace TMS.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}