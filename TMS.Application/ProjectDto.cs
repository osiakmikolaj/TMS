using AutoMapper;
using TMS.Domain.Entities;

namespace TMS.Application.Common.Mappings;

public interface IMapFrom<T>
{
    void Mapping(AutoMapper.Profile profile) => profile.CreateMap(typeof(T), GetType());
}

public class ProjectDto : IMapFrom<Project>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TicketsCount { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Project, ProjectDto>()
            .ForMember(d => d.TicketsCount, opt => opt.MapFrom(s => s.Tickets.Count));
    }
}