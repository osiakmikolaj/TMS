using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.API.Controllers;
using TMS.Domain.Entities;
using TMS.Infrastructure.Persistence;
using Xunit;

namespace TMS.API.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public UserControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
        }

        private async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            var users = new List<User>
            {
                new User {
                    Username = "user1",
                    Email = "user1@example.com",
                    FirstName = "User",
                    LastName = "One",
                    Role = "Administrator"
                },
                new User {
                    Username = "user2",
                    Email = "user2@example.com",
                    FirstName = "User",
                    LastName = "Two",
                    Role = "Developer"
                }
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new UserController(context);

            // Act
            var result = await controller.GetUsers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(actionResult.Value);
            Assert.Equal(2, users.Count());
        }

        [Fact]
        public async Task GetUser_ReturnsUser_WhenUserExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new UserController(context);
            var user = await context.Users.FirstAsync();

            // Act
            var result = await controller.GetUser(user.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var returnValue = Assert.IsType<User>(actionResult.Value);
            Assert.Equal(user.Id, returnValue.Id);
            Assert.Equal(user.Username, returnValue.Username);
            Assert.Equal(user.Email, returnValue.Email);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            await SeedDatabaseAsync(context);
            var controller = new UserController(context);
            var nonExistentId = 9999;

            // Act
            var result = await controller.GetUser(nonExistentId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateUser_AddsUserToDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var controller = new UserController(context);
            var newUser = new User
            {
                Username = "newuser",
                Email = "newuser@example.com",
                FirstName = "New",
                LastName = "User",
                Role = "Developer"
            };

            // Act
            var result = await controller.CreateUser(newUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<User>(createdAtActionResult.Value);
            Assert.Equal(newUser.Username, returnValue.Username);

            // Sprawdź, czy użytkownik został dodany do bazy
            using var verificationContext = new ApplicationDbContext(_dbContextOptions);
            var userInDb = await verificationContext.Users.FindAsync(returnValue.Id);
            Assert.NotNull(userInDb);
            Assert.Equal(newUser.Username, userInDb.Username);
            Assert.Equal(newUser.Email, userInDb.Email);
        }
    }
}