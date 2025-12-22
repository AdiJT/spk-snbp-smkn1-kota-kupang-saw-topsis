using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Auth;
using SpkSnbp.Infrastructure.Configurations;
using SpkSnbp.Infrastructure.Database;
using SpkSnbp.Infrastructure.ModulUtama;
using SpkSnbp.Infrastructure.Services.FileServices;

namespace SpkSnbp.Infrastructure;

public static class DepedencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new NullReferenceException("connection string 'Default' is null");

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .EnableSensitiveDataLogging());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITahunAjaranRepository, TahunAjaranRepository>();
        services.AddScoped<IJurusanRepository, JurusanRepository>();
        services.AddScoped<ISiswaRepository, SiswaRepository>();
        services.AddScoped<IKriteriaRepository, KriteriaRepository>();
        services.AddScoped<ISiswaKriteriaRepository, SiswaKriteriaRepository>();

        services.Configure<FileConfigurationOptions>(configuration.GetSection(FileConfigurationOptions.FileConfiguration));
        services.AddScoped(sp => sp.GetRequiredService<IOptionsSnapshot<FileConfigurationOptions>>().Value);
        services.AddScoped<IFileService, FileService>();

        return services;
    }
}
