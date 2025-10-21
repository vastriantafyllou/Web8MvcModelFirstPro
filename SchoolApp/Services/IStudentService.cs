using SchoolApp.Data;
using SchoolApp.Dto;
using SchoolApp.Models;

namespace SchoolApp.Services
{
    public interface IStudentService
    {
        Task<PaginatedResult<UserReadOnlyDto>> GetPaginatedStudentsAsync(int pageNumber, int pageSize);
    }
}