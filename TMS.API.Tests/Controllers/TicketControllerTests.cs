using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.API.Controllers;
using TMS.Domain.Entities;
using TMS.Infrastructure.Persistence;
using Xunit;

namespace TMS.API.Tests.Controllers
{
    public class TicketControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public TicketControllerTests()
        {
            // Konfiguracja bazy danych w pamięci dla testów
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
        }

        private async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            // Dodaj użytkowników testowych
            var adminUser = new User
            {
                Username = "testadmin",
                Email = "testadmin@example.com",
                FirstName = "Test",
                LastName = "Admin",
                Role = "Administrator"
            };

            var developerUser = new User
            {
                Username = "testdev",
                Email = "testdev@example.com",
                FirstName = "Test",
                LastName = "Developer",
                Role = "Developer"
            };

            context.Users.AddRange(adminUser, developerUser);
            await context.SaveChangesAsync();

            // Dodaj projekt testowy
            var project = new Project
            {
                Name = "Test Project",
                Description = "Test Project Description"
            };
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            // Dodaj zgłoszenia testowe
            var tickets = new List<Ticket>
            {
                new Ticket
                {
                    Title = "Test Ticket 1",
                    Description = "Test Description 1",
                    Status = TicketStatus.New,
                    CreatedById = adminUser.Id,
                    ProjectId = project.Id,
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new Ticket
                {
                    Title = "Test Ticket 2",
                    Description = "Test Description 2",
                    Status = TicketStatus.InProgress,
                    CreatedById = adminUser.Id,
                    AssignedToId = developerUser.Id,
                    ProjectId = project.Id,
                    CreatedAt = DateTime.Now.AddDays(-1)
                }
            };

            context.Tickets.AddRange(tickets);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetTickets_ReturnsAllTickets()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new TicketController(context);

            // Act
            var result = await controller.GetTickets();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Ticket>>>(result);
            var tickets = Assert.IsAssignableFrom<IEnumerable<Ticket>>(actionResult.Value);
            Assert.Equal(2, tickets.Count());
        }

        [Fact]
        public async Task GetTicket_ReturnsTicket_WhenTicketExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new TicketController(context);
            var ticket = await context.Tickets.FirstAsync();

            // Act
            var result = await controller.GetTicket(ticket.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Ticket>>(result);
            var returnValue = Assert.IsType<Ticket>(actionResult.Value);
            Assert.Equal(ticket.Id, returnValue.Id);
            Assert.Equal(ticket.Title, returnValue.Title);
        }

        [Fact]
        public async Task GetTicket_ReturnsNotFound_WhenTicketDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new TicketController(context);
            var nonExistentId = 9999;

            // Act
            var result = await controller.GetTicket(nonExistentId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateTicket_AddsTicketToDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new TicketController(context);
            var project = await context.Projects.FirstAsync();
            var user = await context.Users.FirstAsync();

            var newTicket = new Ticket
            {
                Title = "New Test Ticket",
                Description = "New Test Description",
                Status = TicketStatus.New,
                CreatedById = user.Id,
                ProjectId = project.Id
            };

            // Act
            var result = await controller.CreateTicket(newTicket);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Ticket>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Ticket>(createdAtActionResult.Value);
            Assert.Equal(newTicket.Title, returnValue.Title);
            Assert.Equal(newTicket.Description, returnValue.Description);

            // Sprawdź, czy ticket został dodany do bazy
            using var verificationContext = new ApplicationDbContext(_dbContextOptions);
            var ticketInDb = await verificationContext.Tickets.FindAsync(returnValue.Id);
            Assert.NotNull(ticketInDb);
            Assert.Equal(newTicket.Title, ticketInDb.Title);
        }

        [Fact]
        public async Task UpdateTicket_UpdatesTicketInDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new TicketController(context);
            var ticket = await context.Tickets.FirstAsync();

            // Modyfikacja zgłoszenia
            ticket.Title = "Updated Title";
            ticket.Status = TicketStatus.Completed;

            // Act
            var result = await controller.UpdateTicket(ticket.Id, ticket);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Sprawdź, czy ticket został zaktualizowany w bazie
            using var verificationContext = new ApplicationDbContext(_dbContextOptions);
            var updatedTicket = await verificationContext.Tickets.FindAsync(ticket.Id);
            Assert.NotNull(updatedTicket);
            Assert.Equal("Updated Title", updatedTicket.Title);
            Assert.Equal(TicketStatus.Completed, updatedTicket.Status);
        }

        [Fact]
        public async Task DeleteTicket_RemovesTicketFromDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new TicketController(context);
            var ticket = await context.Tickets.FirstAsync();

            // Act
            var result = await controller.DeleteTicket(ticket.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Sprawdź, czy ticket został usunięty z bazy
            using var verificationContext = new ApplicationDbContext(_dbContextOptions);
            var deletedTicket = await verificationContext.Tickets.FindAsync(ticket.Id);
            Assert.Null(deletedTicket);
        }
    }
}