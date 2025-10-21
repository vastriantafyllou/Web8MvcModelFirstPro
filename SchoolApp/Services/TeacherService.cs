using AutoMapper;
using SchoolApp.Core.Enums;
using SchoolApp.Data;
using SchoolApp.Dto;
using SchoolApp.Exceptions;
using SchoolApp.Repositories;
using SchoolApp.Security;
using Serilog;

namespace SchoolApp.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherService> _logger = new LoggerFactory().AddSerilog().CreateLogger<TeacherService>();

        public TeacherService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task SignUpUserAsync(TeacherSignupDto request)
        {
            var teacher = ExtractTeacher(request);
            var user = ExtractUser(request);

            try
            {
                //user = ExtractUser(request);
                User? existingUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(user.Username);

                if (existingUser != null)
                {
                    throw new EntityAlreadyExistsException("User", "User with username " +
                        existingUser.Username + " already exists");
                }
                
                User? existingUserEmail = await _unitOfWork.UserRepository.GetUserByEmailAsync(user.Email);
                if (existingUserEmail != null)
                {
                    throw new EntityAlreadyExistsException("User", "User with email " +
                                                                   existingUserEmail.Email + " already exists");
                }

                user.Password = EncryptionUtil.Encrypt(user.Password);
                await _unitOfWork.UserRepository.AddAsync(user);

    
                //if (await _unitOfWork.TeacherRepository.GetByPhoneNumberAsync(teacher.PhoneNumber) is not null)
                //{
                //    throw new EntityAlreadyExistsException("Teacher", "Teacher with phone number " +
                //        teacher.PhoneNumber + " already exists");
                //}

                await _unitOfWork.TeacherRepository.AddAsync(teacher);
                user.Teacher = teacher;
                // teacher.User = user; EF manages the other-end of the relationship since both entities are attached

                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Teacher {Teacher} signed up successfully.", teacher);        // ToDo toString in Teacher
            }
            catch (EntityAlreadyExistsException ex)
            {
                _logger.LogError("Error signing up teacher {Teacher}. {Message}", teacher, ex.Message);
                throw;
            }
        }

        private User ExtractUser(TeacherSignupDto signupDto)
        {
            return new User()
            {
                Username = signupDto.Username!,
                Password = signupDto.Password!,
                Email = signupDto.Email!,
                Firstname = signupDto.Firstname!,
                Lastname = signupDto.Lastname!,
                //UserRole = signupDto.UserRole
                UserRole = UserRole.Teacher
            };
        }

        private Teacher ExtractTeacher(TeacherSignupDto signupDto)
        {
            return new Teacher()
            {
                PhoneNumber = signupDto.PhoneNumber!,
                Institution = signupDto.Institution!
            };
        }
    }
}