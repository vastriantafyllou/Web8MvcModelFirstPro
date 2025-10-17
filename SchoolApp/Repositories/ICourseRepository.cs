using SchoolApp.Data;

namespace SchoolApp.Repositories;

public interface ICourseRepository
{
    Task<List<Student>> GetCourseStudentsAsync(int courseId);
    Task<Teacher?> GetCourseTeacherAsync(int courseId);
}