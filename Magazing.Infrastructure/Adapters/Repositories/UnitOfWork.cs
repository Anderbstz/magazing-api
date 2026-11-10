using Magazing.Domain.Repositories;
using Magazing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Otro usuario modifico estos datos al mismo tiempo. Vuelva a intentarlo.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new InvalidOperationException("Ya existe un registro con esos datos.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.CheckViolation })
        {
            throw new InvalidOperationException("La operacion viola una regla de la base de datos.");
        }
    }
}
