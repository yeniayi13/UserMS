namespace UserMs.Application.Dtos.Users.Response{
    public class GetUsersDto
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = String.Empty;
        public string? UsersType { get; init; }
        public string UserPassword { get; set; } = String.Empty;
        public string? UserName { get; set; } = String.Empty;
        public string? UserPhone { get; set; } = String.Empty;
        public string? UserAddress { get; set; } = String.Empty;
        public string? UserAvailable { get; init; }
        public string? UserLastName { get; set; } = String.Empty;
        public bool UserDelete { get; set; } = false;
    }
}