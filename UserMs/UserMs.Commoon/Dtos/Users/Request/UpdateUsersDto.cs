
namespace UserMs.Commoon.Dtos.Users.Request{
    public class UpdateUsersDto
    {
        public string UserEmail { get; set; } = String.Empty;
        public UsersType UsersType { get; init; }
        public UserAvailable UsersAvailable { get; init; }
        public string UserPassword { get; set; } = String.Empty;
        public string? UserName { get; set; } = String.Empty;
        public string? UserPhone { get; set; } = String.Empty;
        public string? UserAddress { get; set; } = String.Empty;
        public string? UserLastName { get; set; } = String.Empty;
    }
}