using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request;

namespace UserMs.Core.Service.Keycloak
{
    public interface IKeycloakService
    {
        Task<string> GetTokenAsync();
        Task<string> LoginAsync(string username, string password);
        Task<string> LogOutAsync();
        Task<string> CreateUserAsync(string userEmail, string userPassword,string userName, string userLastName, string userPhone, string userAddress);
        Task<string> DeleteUserAsync(Guid userId);
        Task RemoveClientRoleFromUser(Guid userId, string roleName);
        Task AssignClientRoleToUser(
            Guid userId,
            string roleName
        );

        //Task<string> ActivateUser(Guid userId);
        Task<Guid> GetUserByUserName(string userName);
        Task UpdateUser(Guid id, UpdateUserDto userDto);

        //Task AssignClientRoleToUserMobile(Guid userId, string clientId, string roleName);
        // Task<string> GetUserNameById(Guid userId);
        // Task<bool> ValidateUserExists(Guid userId);
       // Task<bool> ModificarPermisosRol(string rolId, List<string> permisos, bool agregar);
        Task<string> GetTokenAdministrator();
        Task<bool> SendPasswordResetEmailAsync(string userEmail);
    }
}
