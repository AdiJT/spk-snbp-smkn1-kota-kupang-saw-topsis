using SpkSnbp.Domain.Abstracts;

namespace SpkSnbp.Domain.Auth;

public class User : Entity<int>
{
    public required string UserName { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; }
}

public interface IUserRepository
{
    Task<User?> Get(int id);
    Task<User?> Get(string username);
    Task<List<User>> GetAll();
    Task<bool> IsExist(string username, int? idFilter = null);

    void Add(User user);
    void Update(User user);
    void Delete(User user);
}