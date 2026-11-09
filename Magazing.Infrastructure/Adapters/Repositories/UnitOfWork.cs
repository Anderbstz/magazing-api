using Magazing.Domain.Repositories;
using Magazing.Infrastructure.Context;

namespace Magazing.Infrastructure.Adapters.Repositories;

public class UnitOfWork(MagazingDbContext context) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = new();

    public IRepository<T> Repository<T>() where T : class
    {
        if (!_repositories.TryGetValue(typeof(T), out var repository))
        {
            repository = new GenericRepository<T>(context);
            _repositories[typeof(T)] = repository;
        }

        return (IRepository<T>)repository;
    }

    public Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
