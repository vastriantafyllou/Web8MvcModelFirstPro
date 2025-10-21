using AutoMapper;
using SchoolApp.Core.Filters;
using SchoolApp.Data;
using SchoolApp.Dto;
using SchoolApp.Exceptions;
using SchoolApp.Models;
using SchoolApp.Repositories;
using Serilog;
using System.Linq.Expressions;

namespace SchoolApp.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<UserService> logger = 
            new LoggerFactory().AddSerilog().CreateLogger<UserService>();

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PaginatedResult<UserReadOnlyDto>> GetPaginatedUsersFilteredAsync(int pageNumber, 
            int pageSize, UserFiltersDto userFiltersDto)
        {
            List<User> users = [];
            List<Expression<Func<User, bool>>> predicates = [];

            if (!string.IsNullOrEmpty(userFiltersDto.Username))
            {
                predicates.Add(u => u.Username == userFiltersDto.Username);
            }
            if (!string.IsNullOrEmpty(userFiltersDto.Email))
            {
                predicates.Add(u => u.Email == userFiltersDto.Email);
            }
            if (!string.IsNullOrEmpty(userFiltersDto.UserRole))
            {
                predicates.Add(u => u.UserRole.ToString() == userFiltersDto.UserRole);
            }

            var result = await unitOfWork.UserRepository.GetUsersAsync(pageNumber, pageSize, predicates);
            
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
            logger.LogInformation("Retrieved {Count} users", dtoResult.Data.Count);
            return dtoResult;
        }

        public async Task<UserReadOnlyDto?> GetUserByUsernameAsync(string username)
        {
            try
            {
                User? user = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    throw new EntityNotFoundException("User", "User with username: " + " not found");
                }
                
                logger.LogInformation("User found: {Username}", username);
                return new UserReadOnlyDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    UserRole = user.UserRole.ToString()!
                };
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogError("Error retrieving user by username: {Username}. {Message}", username, ex.Message);
                throw;
            }
        }

        public async Task<User?> VerifyAndGetUserAsync(UserLoginDto credentials)
        {
            User? user = null;
            try
            {
                user = await unitOfWork.UserRepository.GetUserAsync(credentials.Username!, credentials.Password!);

                if (user == null)
                {
                    //throw new EntityNotAuthorizedException("User", "Bad Credentials");
                    // see Resources/Strings.resx for localization
                    throw new EntityNotAuthorizedException("User", Resources.ErrorMessages.BadCredentials); 
                }

                logger.LogInformation("User with username {Username} found", credentials.Username!);
            }
            catch (EntityNotAuthorizedException e)
            {
                logger.LogError("Authentication failed for username {Username}. {Message}",
                    credentials.Username, e.Message);
            }
            return user;
        }
    }
}