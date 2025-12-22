using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.Shared;

namespace SpkSnbp.Infrastructure.Database;

internal class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _appDbContext;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(AppDbContext appDbContext, ILogger<UnitOfWork> logger)
    {
        _appDbContext = appDbContext;
        _logger = logger;
    }

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            UpdateAuditableEntity();
            await _appDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured when try to save changes to storage");

            return new Error(
                "IUnitOfWork.SaveChangesAsyncGagal",
                $"Penyimpanan ke database gagal karena kesalahan tidak terduga (Exception : {ex.Message})");
        }
    }

    public void UpdateAuditableEntity()
    {
        var entities = _appDbContext.ChangeTracker.Entries<IAuditableEntity>().ToList();

        foreach (var entity in entities.Where(e => e.State == EntityState.Added))
        {
            entity.Entity.CreatedAt = DateTime.Now;
            entity.Entity.UpdatedAt = entity.Entity.CreatedAt;
        }

        foreach (var entity in entities.Where(e => e.State == EntityState.Modified))
            entity.Entity.UpdatedAt = DateTime.Now;
    }
}
