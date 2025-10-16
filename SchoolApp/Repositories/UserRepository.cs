using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;
using SchoolApp.Security;

namespace SchoolApp.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(SchoolAppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetUserAsync(string username, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username 
                                                                || u.Email == username);
        
        if (user == null) return null;
        if (!EncryptionUtil.IsValidPassword(password,  user.Password)) return null;
        return user;
    }

    public async Task<User?> GetUserByUsernameAsync(string username) =>
        await context.Users.FirstOrDefaultAsync(u => u.Username == username);
    
    public async Task<User?> GetUserByEmailAsync(string email) =>
        await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    
    public async Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize, List<Expression<Func<User, bool>>> predicates)
    {
        IQueryable<User> query = context.Users; // Δεν εκτελείται ακόμα

        if (predicates != null && predicates.Count > 0)
        {
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }
        }

        int totalRecords = await query.CountAsync(); // εκτελείται
        
        int skip = (pageNumber - 1) * pageSize;
        
        var data = await query
            .OrderBy(u => u.Id) // Παντα υπάρχει OrderBy πριν το Skip
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(); // εκτελείται
            
        return new PaginatedResult<User>(data, totalRecords, pageNumber, pageSize);
    }
}