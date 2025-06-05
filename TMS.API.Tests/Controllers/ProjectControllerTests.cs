using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.API.Controllers;
using TMS.Domain.Entities;
using TMS.Infrastructure.Persistence;
using Xunit;

namespace TMS.API.Tests.Controllers
{
    public class ProjectControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public ProjectControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
        }

        private async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            var projects = new List<Project>
            {
                new Project { Name = "Project 1", Description = "Description 1" },
                new Project { Name = "Project 2", Description = "Description 2" }
            };

            context.Projects.AddRange(projects);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetProjects_ReturnsAllProjects()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new ProjectController(context);

            // Act
            var result = await controller.GetProjects();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Project>>>(result);
            var projects = Assert.IsAssignableFrom<IEnumerable<Project>>(actionResult.Value);
            Assert.Equal(2, projects.Count());
        }

        [Fact]
        public async Task GetProject_ReturnsProject_WhenProjectExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new ProjectController(context);
            var project = await context.Projects.FirstAsync();

            // Act
            var result = await controller.GetProject(project.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Project>>(result);
            var returnValue = Assert.IsType<Project>(actionResult.Value);
            Assert.Equal(project.Id, returnValue.Id);
            Assert.Equal(project.Name, returnValue.Name);
        }

        [Fact]
        public async Task GetProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new ProjectController(context);
            var nonExistentId = 9999;

            // Act
            var result = await controller.GetProject(nonExistentId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateProject_AddsProjectToDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var controller = new ProjectController(context);
            var newProject = new Project { Name = "New Project", Description = "New Description" };

            // Act
            var result = await controller.CreateProject(newProject);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Project>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Project>(createdAtActionResult.Value);
            Assert.Equal(newProject.Name, returnValue.Name);

            // Sprawdź, czy projekt został dodany do bazy
            using var verificationContext = new ApplicationDbContext(_dbContextOptions);
            var projectInDb = await verificationContext.Projects.FindAsync(returnValue.Id);
            Assert.NotNull(projectInDb);
            Assert.Equal(newProject.Name, projectInDb.Name);
        }
    }
}