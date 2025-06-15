using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using TMS.Application.Common.Interfaces;
using TMS.Application.Common.Mappings;
using TMS.Application.Projects.Queries.GetProjects;
using TMS.Domain.Entities;

namespace TMS.Application.Tests.Projects.Queries.GetProjects
{
    public static class MockDbSetHelper
    {
        public static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();

            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            dbSet.As<IAsyncEnumerable<T>>().Setup(d => d.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            dbSet.Setup(x => x.AsQueryable()).Returns(queryable);


            return dbSet;
        }
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                                  .GetMethods()
                                  .First(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
                                  .MakeGenericMethod(resultType)
                                  .Invoke(this, new object[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                                .MakeGenericMethod(resultType)
                                .Invoke(null, new[] { executionResult });
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public T Current => _inner.Current;

        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
    }


    public class GetProjectsQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly IMapper _mapper;
        private readonly GetProjectsQueryHandler _handler;

        public GetProjectsQueryHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.CreateMap<Project, ProjectDto>()
                  .ForMember(d => d.TicketsCount, opt => opt.MapFrom(s => s.Tickets.Count));
            });
            _mapper = mappingConfig.CreateMapper();

            _handler = new GetProjectsQueryHandler(_mockContext.Object, _mapper);
        }

        [Fact]
        public async Task Handle_Should_ReturnListOfProjectDtos_WhenProjectsExist()
        {
            var projects = new List<Project>
            {
                new Project { Id = 1, Name = "Project Alpha", Description = "Desc Alpha", Tickets = new List<Ticket>() },
                new Project { Id = 2, Name = "Project Beta", Description = "Desc Beta", Tickets = new List<Ticket> { new Ticket(), new Ticket() } }
            };

            var mockProjectsDbSet = MockDbSetHelper.CreateMockDbSet(projects);
            _mockContext.Setup(c => c.Projects).Returns(mockProjectsDbSet.Object);

            var query = new GetProjectsQuery();

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            Assert.Equal("Project Alpha", result.First(p => p.Id == 1).Name);
            Assert.Equal(0, result.First(p => p.Id == 1).TicketsCount);

            Assert.Equal("Project Beta", result.First(p => p.Id == 2).Name);
            Assert.Equal(2, result.First(p => p.Id == 2).TicketsCount);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoProjectsExist()
        {
            var projects = new List<Project>();
            var mockProjectsDbSet = MockDbSetHelper.CreateMockDbSet(projects);
            _mockContext.Setup(c => c.Projects).Returns(mockProjectsDbSet.Object);

            var query = new GetProjectsQuery();

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}