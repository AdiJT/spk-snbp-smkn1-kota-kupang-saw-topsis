using SpkSnbp.Domain.Shared;

namespace SpkSnbp.Domain.Contracts;

public interface IUnitOfWork
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
