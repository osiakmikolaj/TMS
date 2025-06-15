using Moq;
using TMS.Application.Common.Interfaces;
using TMS.Application.Projects.Commands.CreateProject;
using TMS.Domain.Entities;

namespace TMS.Application.Tests.Projects.Commands.CreateProject
{
    public class CreateProjectCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly CreateProjectCommandHandler _handler;

        public CreateProjectCommandHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new CreateProjectCommandHandler(_mockContext.Object);
        }

        [Fact]
        public async Task Handle_Should_AddProjectToContextAndSaveChanges_WhenCalled()
        {
            var command = new CreateProjectCommand
            {
                Name = "Test Project",
                Description = "Test Description"
            };

            var createdProject = new Project();

            _mockContext.Setup(c => c.Projects.Add(It.IsAny<Project>()))
                        .Callback<Project>(p => createdProject = p);

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            _mockContext.Verify(c => c.Projects.Add(It.Is<Project>(p =>
                p.Name == command.Name &&
                p.Description == command.Description
            )), Times.Once);

            _mockContext.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);

            Assert.Equal(createdProject.Id, result);
        }
    }
}