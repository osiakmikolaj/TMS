using MediatR;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Projects.Commands.CreateProject;
using TMS.Application.Projects.Queries.GetProjects;

namespace TMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetProjects()
    {
        return await _mediator.Send(new GetProjectsQuery());
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateProjectCommand command)
    {
        return await _mediator.Send(command);
    }
}