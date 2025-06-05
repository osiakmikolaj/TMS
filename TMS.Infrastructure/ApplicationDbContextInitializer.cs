using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMS.Domain.Entities;

namespace TMS.Infrastructure.Persistence;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsInMemory())
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wystąpił błąd podczas inicjalizacji bazy danych");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wystąpił błąd podczas wprowadzania danych początkowych do bazy");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Dodanie użytkowników
        if (!_context.Users.Any())
        {
            _context.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@tms.com",
                FirstName = "Admin",
                LastName = "System",
                Role = "Administrator"
            });

            _context.Users.Add(new User
            {
                Username = "jan.kowalski",
                Email = "jan.kowalski@tms.com",
                FirstName = "Jan",
                LastName = "Kowalski",
                Role = "Developer"
            });

            await _context.SaveChangesAsync();
        }

        // Dodanie projektów
        if (!_context.Projects.Any())
        {
            _context.Projects.Add(new Project
            {
                Name = "System Zarządzania Magazynem",
                Description = "System do zarządzania stanem magazynowym firmy logistycznej."
            });

            _context.Projects.Add(new Project
            {
                Name = "Portal Klienta",
                Description = "Portal internetowy dla klientów do zarządzania zamówieniami."
            });

            await _context.SaveChangesAsync();
        }

        // Dodanie ticketów
        if (!_context.Tickets.Any())
        {
            var adminUser = await _context.Users.FirstAsync(u => u.Username == "admin");
            var developer = await _context.Users.FirstAsync(u => u.Username == "jan.kowalski");
            var project1 = await _context.Projects.FirstAsync(p => p.Name == "System Zarządzania Magazynem");
            var project2 = await _context.Projects.FirstAsync(p => p.Name == "Portal Klienta");

            _context.Tickets.Add(new Ticket
            {
                Title = "Implementacja logowania",
                Description = "Implementacja modułu logowania z uwierzytelnianiem JWT",
                Status = TicketStatus.InProgress,
                CreatedById = adminUser.Id,
                AssignedToId = developer.Id,
                ProjectId = project1.Id,
                CreatedAt = DateTime.Now.AddDays(-5)
            });

            _context.Tickets.Add(new Ticket
            {
                Title = "Optymalizacja wyszukiwania",
                Description = "Optymalizacja algorytmu wyszukiwania produktów w magazynie",
                Status = TicketStatus.New,
                CreatedById = adminUser.Id,
                ProjectId = project1.Id,
                CreatedAt = DateTime.Now.AddDays(-2)
            });

            _context.Tickets.Add(new Ticket
            {
                Title = "Projekt interfejsu klienta",
                Description = "Stworzenie projektu UI dla portalu klienta",
                Status = TicketStatus.Completed,
                CreatedById = adminUser.Id,
                AssignedToId = developer.Id,
                ProjectId = project2.Id,
                CreatedAt = DateTime.Now.AddDays(-10),
                UpdatedAt = DateTime.Now.AddDays(-3)
            });

            await _context.SaveChangesAsync();
        }
    }
}