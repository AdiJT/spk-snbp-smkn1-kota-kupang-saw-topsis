using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Infrastructure.Database;

namespace SpkSnbp.Infrastructure.Auth;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasData(
            new User
            {
                Id = 1,
                UserName = "Admin",
                PasswordHash = "AQAAAAIAAYagAAAAEKXsR8woVHO5DgmyBgmfe5b4I7jeJZYtk71JFY4HkDSCsimeHtIwzOueTyHo8gBH/A==",
                Role = UserRoles.Admin
            },
            new User
            {
                Id = 2,
                UserName = "Wali Kelas",
                PasswordHash = "AQAAAAIAAYagAAAAEKXsR8woVHO5DgmyBgmfe5b4I7jeJZYtk71JFY4HkDSCsimeHtIwzOueTyHo8gBH/A==",
                Role = UserRoles.WaliKelas
            },
            new User
            {
                Id = 3,
                UserName = "Kepala Sekolah",
                PasswordHash = "AQAAAAIAAYagAAAAEKXsR8woVHO5DgmyBgmfe5b4I7jeJZYtk71JFY4HkDSCsimeHtIwzOueTyHo8gBH/A==",
                Role = UserRoles.KepalaSekolah
            }
        );
    }
}

internal class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;

    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Add(User user) => _appDbContext.User.Add(user);

    public void Delete(User user) => _appDbContext.User.Remove(user);

    public async Task<User?> Get(int id) => await _appDbContext
        .User
        .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<User?> Get(string username) => await _appDbContext
        .User
        .FirstOrDefaultAsync(x => x.UserName == username);

    public async Task<List<User>> GetAll() => await _appDbContext
        .User
        .ToListAsync();

    public async Task<bool> IsExist(string username, int? idFilter = null) => await _appDbContext
        .User
        .AnyAsync(x => x.Id != idFilter && x.UserName == username);

    public void Update(User user) => _appDbContext.User.Update(user);
}
