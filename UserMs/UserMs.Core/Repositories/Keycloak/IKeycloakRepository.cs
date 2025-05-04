using UserMs.Commoon.Dtos;
using UserMs.Common.Dtos.Users.Request;

namespace UserMs.Core
{
    public interface IKeycloakRepository
    {
       // Task<string> GetTokenAsync();
        Task<string> LoginAsync(string username, string password);
        Task<string> LogOutAsync();
        Task<string> CreateUserAsync(string userEmail, string userPassword);
        Task<string> DeleteUserAsync(Guid userId);

        Task AssignClientRoleToUser(
            Guid userId,
            string clientId,
            string roleName
        );
        Task<Guid> GetUserByUserName(string userName);
        Task UpdateUser(Guid id, UpdateUserDto userDto);

        //Task AssignClientRoleToUserMobile(Guid userId, string clientId, string roleName);
        // Task<string> GetUserNameById(Guid userId);
        // Task<bool> ValidateUserExists(Guid userId);
    }
}
