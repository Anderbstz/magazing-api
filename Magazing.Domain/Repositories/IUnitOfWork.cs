namespace Magazing.Domain.Repositories;

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;

    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}
