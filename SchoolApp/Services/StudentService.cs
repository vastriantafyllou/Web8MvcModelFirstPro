using AutoMapper;
using SchoolApp.Dto;
using SchoolApp.Models;
using SchoolApp.Repositories;
using Serilog;

namespace SchoolApp.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<StudentService> logger = new LoggerFactory().AddSerilog().CreateLogger<StudentService>();

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PaginatedResult<UserReadOnlyDto>> GetPaginatedStudentsAsync(int pageNumber, int pageSize)
        {
            var result = await unitOfWork.StudentRepository.GetPaginatedUsersStudentsAsync(pageNumber, pageSize);

            var dtoResult = new PaginatedResult<UserReadOnlyDto>()
            {
                Data = result.Data.Select(u => new UserReadOnlyDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Firstname = u.Firstname,
                    Lastname = u.Lastname,
                    UserRole = u.UserRole.ToString()!
                }).ToList(),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
            logger.LogInformation("Retrieved {Count} users-students", dtoResult.Data.Count);
            return dtoResult;
        }
    }
}