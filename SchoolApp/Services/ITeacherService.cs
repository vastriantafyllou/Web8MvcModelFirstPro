using SchoolApp.Dto;

namespace SchoolApp.Services
{
    public interface ITeacherService
    {
        Task SignUpUserAsync(TeacherSignupDto request);
    }
}