using AuthService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure;

public class EfAuthRepository : IAuthRepository
{
    private readonly AuthDbContext _db;

    public EfAuthRepository(AuthDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<User>> GetUsersAsync() =>
        await _db.Users.AsNoTracking().ToListAsync();

    public async Task<User?> GetUserByUsernameAsync(string username) =>
        await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

    public async Task SaveUserAsync(User user)
    {
        var exists = await _db.Users.AnyAsync(u => u.Id == user.Id);
        if (exists) _db.Users.Update(user);
        else _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }
}
