namespace UserMs.Commoon.Dtos.Users.Request.User{
    public class UpdateUsersDto
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UsersType { get; init; }
        public string UserAvailable { get; init; }
       // public string UserPassword { get; set; } = string.Empty;
        public string? UserName { get; set; } = string.Empty;
        public string? UserPhone { get; set; } = string.Empty;
        public string? UserAddress { get; set; } = string.Empty;
        public string? UserLastName { get; set; } = string.Empty;
        public bool UserDelete { get; set; } = false;
    }
}