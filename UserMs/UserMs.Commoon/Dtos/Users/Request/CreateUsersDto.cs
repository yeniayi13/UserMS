

namespace UserMs.Commoon.Dtos.Users.Request
{
    public class CreateUsersDto
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string UserEmail { get; init; } = String.Empty;
        public string UsersType { get; init; }
        public string UserAvailable { get; init; }
        public string UserPassword { get; set; } = String.Empty;
        public string? UserName { get; set; } = String.Empty;
        public string? UserPhone { get; set; } = String.Empty;
        public string? UserAddress { get; set; } = String.Empty;
        public string? UserLastName { get; set; } = String.Empty;

    }
}