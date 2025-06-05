using Microsoft.EntityFrameworkCore;
using TMS.Domain.Entities;

namespace TMS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Ticket> Tickets { get; }
    DbSet<Project> Projects { get; }
    DbSet<User> Users { get; }
    DbSet<Comment> Comments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}