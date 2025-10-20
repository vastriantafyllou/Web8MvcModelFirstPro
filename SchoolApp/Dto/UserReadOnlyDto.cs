namespace SchoolApp.Dto
{
    public record UserReadOnlyDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Firstname { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public string UserRole { get; set; } = null!;
    }
}