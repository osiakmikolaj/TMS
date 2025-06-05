using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TMS.Application.Common.Interfaces;
using TMS.Application.Common.Mappings;

namespace TMS.Application.Projects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<List<ProjectDto>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProjectsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Projects
            .AsNoTracking()
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}