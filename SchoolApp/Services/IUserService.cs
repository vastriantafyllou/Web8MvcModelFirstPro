using SchoolApp.Core.Filters;
using SchoolApp.Data;
using SchoolApp.Dto;
using SchoolApp.Models;

namespace SchoolApp.Services
{
    public interface IUserService
    {
        Task<User?> VerifyAndGetUserAsync(UserLoginDto credentials);
        Task<UserReadOnlyDto?> GetUserByUsernameAsync(string username);
        Task<PaginatedResult<UserReadOnlyDto>> GetPaginatedUsersFilteredAsync(int pageNumber, 
            int pageSize, UserFiltersDto userFiltersDto);
    }
}