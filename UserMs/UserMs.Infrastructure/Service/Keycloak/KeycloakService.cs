
using AuthMs.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using UserMs.Commoon.Dtos;
using UserMs.Infrastructure.Exceptions;
using UserMs.Commoon.Dtos.Users.Request;
using UserMs.Domain.Entities;
using System.Data;
using UserMs.Core.Service.Keycloak;
using Microsoft.IdentityModel.Tokens;
using UserMs.Core;
using System.Security.Authentication;
using System.Net.Http.Headers;

namespace UserMs.Infrastructure.Service.Keycloak
{
    public class KeycloakService : IKeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
       
        private readonly string _httpClientUrl;
        private readonly string _realm = "auth-demo";
        private readonly string _baseUrl = "http://localhost:18080";
        private readonly string _adminUsername = "test@test.com";
        private readonly string _adminPassword = "1234";



        public KeycloakService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptions<HttpClientUrl> httpClientUrl)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _httpClientUrl = httpClientUrl.Value.ApiUrl;

            //* Configuracion del HttpClient
            var headerToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
            _httpClient.BaseAddress = new Uri(_httpClientUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {headerToken}");

           

        }
        //* Me retorna el Authorization TOKEN
        public async Task<string> GetTokenAsync()
        {
            var response = await _httpClient.PostAsync("http://localhost:18080/realms/auth/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", "web-client" },
                { "client_secret", "REdOcKznwuvtZ54jVVt9ebc3nCz6uqMy" }
            }));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;

        }

        public async Task<string> GetTokenAdministrator()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/realms/{_realm}/protocol/openid-connect/token");

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", "admin-cli" },
                { "username", _adminUsername },
                { "password", _adminPassword },
                { "grant_type", "password" }
            });

            var response = await _httpClient.SendAsync(request);
            var contenido = await response.Content.ReadAsStringAsync();

            var tokenJson = JsonDocument.Parse(contenido);
            return tokenJson.RootElement.GetProperty("access_token").GetString();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:18080/realms/auth-demo/protocol/openid-connect/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "client_id", "admin-client" },
        { "client_secret", "QfhngLkKbk6xixmYEzGkXiJc3nvhU0w2" },
        { "grant_type", "client_credentials" }
    });

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new KeycloakException($"Error al obtener token: {response.StatusCode}");
            }

            var json = JsonDocument.Parse(responseString);
            return json.RootElement.GetProperty("access_token").GetString();
        }


        public async Task<string> LoginAsync(string username, string password)
        {
            // 🔹 Validar que `username` y `password` no sean nulos o vacíos
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username), "El nombre de usuario no puede estar vacío.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password), "La contraseña no puede estar vacía.");
            }

            var response = await _httpClient.PostAsync("realms/auth-demo/protocol/openid-connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "client_id", "admin-client" },
                    { "username", username },
                    { "password", password },
                    { "client_secret", "QfhngLkKbk6xixmYEzGkXiJc3nvhU0w2" }
                }));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Error: " + content);

            return content;
        }

        /* public async Task<bool> ModificarPermisosRol(string rolId, List<string> permisos, bool agregar)
         {
             var tokenAdmin = await ObtenerTokenAdministrador();
             var request = new HttpRequestMessage(
                 agregar ? HttpMethod.Post : HttpMethod.Delete,
                 $"{_baseUrl}/admin/realms/{_realm}/roles/{rolId}/composites"
             );

             request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenAdmin);

             var body = permisos.Select(p => new { name = p }).ToArray();
             request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");

             var response = await _httpClient.SendAsync(request);
             return response.IsSuccessStatusCode;
         }*/


        public async Task<string> LogOutAsync()
        {
            try
            {
                var headerToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                    .ToString()?.Replace("Bearer ", "");

                if (string.IsNullOrWhiteSpace(headerToken))
                {
                    throw new ArgumentException("El token es inválido o está vacío.");
                }

                JwtSecurityToken accessToken;
                try
                {
                    accessToken = new JwtSecurityTokenHandler().ReadToken(headerToken) as JwtSecurityToken;
                }
                catch (SecurityTokenMalformedException)
                {
                    throw new SecurityTokenMalformedException("El token tiene un formato incorrecto.");
                }

                var userId = accessToken!.Payload["sub"];
                var client = accessToken.Payload["azp"];

                var response = await _httpClient.PostAsync($"admin/realms/auth-demo/users/{userId}/logout",
                    new FormUrlEncodedContent(new Dictionary<string, string>()));

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error en la solicitud: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (SecurityTokenMalformedException)
            {
                throw; // 🔹 No cambiar el tipo de excepción, solo reenviarla
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Error HTTP en LogOutAsync: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado en LogOutAsync: {ex.Message}");
            }
        }


        public async Task<string> CreateUserAsync(
     string userEmail,
     string userPassword,
     string userName,
     string userLastName,
     string userPhone,
     string userAddress)
        {
            try
            {
                // 🔐 1. Obtener el access token desde Keycloak usando client_credentials
                var accessToken = await GetAccessTokenAsync();

                // 🔧 2. Construir el cuerpo de la solicitud para crear usuario
                var userData = new
                {
                    username = userEmail,
                    email = userEmail,
                    firstName = userName,
                    lastName = userLastName,
                    emailVerified = true,
                    enabled = true,
                    credentials = new[]
                    {
                new
                {
                    type = "password",
                    value = userPassword,
                    temporary = false
                }
            },
                    attributes = new Dictionary<string, object>
            {
                { "phone", new[] { userPhone ?? "" } },
                { "address", new[] { userAddress ?? "" } }
            }
                };

                var jsonBody = JsonSerializer.Serialize(userData);
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // 📤 3. Establecer el header de autorización con el token
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // 🚀 4. Crear el usuario vía Admin API
                var createUserResponse = await _httpClient.PostAsync("http://localhost:18080/admin/realms/auth-demo/users", contentJson);

                if (!createUserResponse.IsSuccessStatusCode)
                {
                    if (createUserResponse.StatusCode == HttpStatusCode.Conflict)
                    {
                        throw new UserExistException("El usuario ya existe. Intenta con otro correo.");
                    }
                    throw new KeycloakException($"Error creando usuario: {createUserResponse.StatusCode}");
                }

                var userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

                // ✉️ 5. Enviar email de activación (opcional)
                var activationResponse = await SendActivationEmail(userId, userEmail);

                return "Usuario creado y enlace de activación enviado con éxito.";
            }
            catch (Exception ex)
            {
                // Puedes loguear el error aquí si prefieres
                throw ex;
            }
        }

        // Método para enviar el enlace de activación
        public async Task<HttpResponseMessage> SendActivationEmail(string userId, string userEmail)
        {
            Console.WriteLine($"Created User ID: {userId}");
          var requestBody = JsonSerializer.Serialize(new[] { "VERIFY_EMAIL" }); // Acción de activación personalizada // Acción de activación
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            //var requestUrl = $"http://localhost:18080/admin/realms/auth-demo/users/{userId}/execute-actions-email";
          
            var response = await _httpClient.PutAsync($"http://localhost:18080/admin/realms/auth-demo/users/{userId}/execute-actions-email", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error enviando el correo de activación: {response.StatusCode}");
            }
            var updateData = new { emailVerified = true, enabled = true };
            var updateJsonBody = JsonSerializer.Serialize(updateData);
            var updateContentJson = new StringContent(updateJsonBody, Encoding.UTF8, "application/json");

            var activateResponse = await _httpClient.PutAsync($"http://localhost:18080/admin/realms/auth-demo/users/{userId}", updateContentJson);

            if (!activateResponse.IsSuccessStatusCode)
            {
                var errorMessage = await activateResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al activar la cuenta después del email: {errorMessage}");
            }

            return response;
        }





        public async Task<string> DeleteUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"admin/realms/auth-demo/users/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new UserNotFoundException("User not found");
                    }
                    throw new KeycloakException("Error on "
                            + _httpClient.BaseAddress
                            + "delete-user "
                            + "Status Code: "
                            + response.StatusCode
                            + "Content : "
                            + response.Content);
                }
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetClientRolesId(string clientId, string userId, string roleName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/admin/realms/auth-demo/users/{userId}/role-mappings/clients/{clientId}/available");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new UserNotFoundException("User not found");
                    }
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new KeycloakException("Error on "
                            + _httpClient.BaseAddress
                            + "create-user"
                            + "Status Code: "
                            + response.StatusCode
                    + "Content : "
                            + response.Content);
                }
                var content = await response.Content.ReadAsStringAsync();
                var json = JArray.Parse(content);
                foreach (var item in json)
                {
                    if (item["name"]!.ToString() == roleName)
                    {
                        return item["id"]!.ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetClientId(string clientId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"admin/realms/auth-demo/clients");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new KeycloakException("Client not found");
                    }
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new KeycloakException("Error on "
                            + _httpClient.BaseAddress
                            + "get-client-id"
                            + "Status Code: "
                            + response.StatusCode
                            + "Content : "
                            + response.Content);
                }
                var content = await response.Content.ReadAsStringAsync();
                var json = JArray.Parse(content);
                foreach (var item in json)
                {
                    if (item["clientId"]!.ToString() == clientId)
                    {
                        return item["id"]!.ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task AssignClientRoleToUser(Guid userId, string roleName)
        {
            try
            {

                Console.WriteLine($"🔍 Verificando rol '{roleName}' para el usuario {userId} en Keycloak...");
                var role = roleName switch
                {
                    "Administrador" => "Administrator",
                    "Subastador" => "Auctioneer",
                    "Postor" => "Bidder",
                    _ => "Support"
                };

                var clientId = await GetClientId("admin-client");

                // 🔹 Verificar si el usuario ya tiene el rol asignado
                var assignedRolesResponse = await _httpClient.GetAsync($"admin/realms/auth-demo/users/{userId}/role-mappings/clients/{clientId}");
                var responseString = await assignedRolesResponse.Content.ReadAsStringAsync();

                // 🔹 Validar si la respuesta está vacía
                if (string.IsNullOrWhiteSpace(responseString))
                {
                    throw new KeycloakException("Error obteniendo roles del usuario: respuesta vacía.");
                }

                List<JsonElement> assignedRoles;
                try
                {
                    assignedRoles = JsonSerializer.Deserialize<List<JsonElement>>(responseString) ?? new List<JsonElement>();
                }
                catch (JsonException ex)
                {
                    throw new KeycloakException($"Error procesando JSON: {ex.Message}");
                }

                // 🔹 Validar si la propiedad "name" existe antes de acceder a ella
                if (assignedRoles.Any(r => r.TryGetProperty("name", out JsonElement nameElement) && nameElement.GetString() == role))
                {
                    throw new InvalidOperationException($"El usuario {userId} ya tiene asignado el rol '{roleName}'.");
                }

                // 🔹 Obtener el ID del rol en Keycloak
                var roleId = await GetClientRolesId(clientId, userId.ToString(), role);
                var roles = new[] { new { id = roleId, name = role } };

                var jsonBody = JsonSerializer.Serialize(roles);
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // 🔹 Asignar el rol si aún no lo tiene
                var response = await _httpClient.PostAsync($"admin/realms/auth-demo/users/{userId}/role-mappings/clients/{clientId}", contentJson);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new KeycloakException($"Error asignando rol en Keycloak: {response.StatusCode} - {errorContent}");
                }

            }
            catch (KeycloakException ex)
            {
                throw new KeycloakException($"Error en Keycloak: {ex.Message}");
            }
            catch (JsonException ex)
            {
                throw new KeycloakException($"Error procesando JSON: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Error de lógica: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error crítico en AssignClientRoleToUser: {ex.Message}");
            }
        }

        public async Task RemoveClientRoleFromUser(string userEmail, string roleName)
        {
            try
            {
                var userId = await GetUserByUserName(userEmail);
                if (userId == Guid.Empty)
                    throw new KeycloakException("❌ No se encontró ningún usuario con el email proporcionado.");

                var role = roleName switch
                {
                    "Administrador" => "Administrator",
                    "Subastador" => "Auctioneer",
                    "Postor" => "Bidder",
                    _ => "Support"
                };

                var clientId = await GetClientId("admin-client");

                // Verificar existencia del rol
                var allRolesResponse = await _httpClient.GetAsync($"admin/realms/auth-demo/clients/{clientId}/roles");
                var allRolesString = await allRolesResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(allRolesString))
                    throw new KeycloakException($"⚠️ No se encontraron roles en Keycloak para el cliente {clientId}.");

                var availableRoles = JsonSerializer.Deserialize<List<JsonElement>>(allRolesString) ?? new();
                var roleExists = availableRoles.Any(r => r.TryGetProperty("name", out var nameEl) && nameEl.GetString() == role);
                if (!roleExists)
                    throw new KeycloakException($"⚠️ El rol '{role}' no existe en Keycloak.");

                // Verificar si el usuario tiene el rol asignado
                var assignedRolesResponse = await _httpClient.GetAsync($"admin/realms/auth-demo/users/{userId}/role-mappings/clients/{clientId}");
                var responseString = await assignedRolesResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseString))
                    throw new KeycloakException($"⚠️ El usuario {userEmail} no tiene roles asignados.");

                var assignedRoles = JsonSerializer.Deserialize<List<JsonElement>>(responseString) ?? new();
                if (!assignedRoles.Any(r => r.TryGetProperty("name", out var nameEl) && nameEl.GetString() == role))
                    throw new KeycloakException($"⚠️ El usuario {userEmail} no tiene el rol '{roleName}' asignado.");

                // Obtener el ID del rol
                var roleId = await GetClientRolesId(clientId, userId.ToString(), role);
                var roles = new[] { new { id = roleId, name = role } };
                var jsonBody = JsonSerializer.Serialize(roles);
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // ¡Aquí está el cambio clave! Usamos POST al endpoint delete
                var request = new HttpRequestMessage(HttpMethod.Post,
                    $"admin/realms/auth-demo/users/{userId}/role-mappings/clients/{clientId}/delete")
                {
                    Content = contentJson
                };

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new KeycloakException($"❌ Error eliminando rol en Keycloak: {response.StatusCode} - {errorContent}");
                }

                Console.WriteLine($"✅ Rol '{roleName}' eliminado exitosamente del usuario {userId} en Keycloak.");
            }
            catch (Exception ex)
            {
                throw new KeycloakException($"❌ Fallo en RemoveClientRoleFromUser: {ex.Message}");
            }
        }


        // public async Task AssignClientRoleToUserMobile(Guid userId, string clientId, string roleName)
        // {
        //     try
        //     {
        //         //* La Client Id es un guid
        //         var mobileId = "3a54e43c-8bd4-401e-bc6f-62ea392c80e6";

        //         var rolId = this.GetClientRolesId(mobileId, userId.ToString(), roleName).Result;
        //         var roles = new[] { new { id = rolId, name = roleName } };
        //         var userData = new { };

        //         // Serializar el objeto a JSON
        //         var jsonBody = JsonSerializer.Serialize(roles);
        //         // Crear el contenido de la solicitud con el encabezado Content-Type
        //         var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        //         var response = await _httpClient.PostAsync($"admin/realms/auth/users/{userId}/role-mappings/clients/{mobileId}", contentJson);

        //         if (!response.IsSuccessStatusCode)
        //         {
        //             var errorContent = await response.Content.ReadAsStringAsync();
        //             throw new KeycloakException("Error on "
        //                     + _httpClient.BaseAddress
        //                     + "assing-role-user"
        //                     + "Status Code: "
        //                     + response.StatusCode
        //                     + "Content : "
        //                     + response.Content);
        //         }
        //         var content = await response.Content.ReadAsStringAsync();
        //     }
        //     catch (Exception ex)
        //     {
        //         throw ex;
        //     }
        // }
        public async Task<Guid> GetUserByUserName(string userName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"admin/realms/auth-demo/users?username={userName}");
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new UserNotFoundException("User not found");
                    }
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new KeycloakException("Error on "
                            + _httpClient.BaseAddress
                            + "get-user-by-username"
                            + "Status Code: "
                            + response.StatusCode
                            + "Content : "
                            + response.Content);
                }

                var responseString = response.Content.ReadAsStringAsync().Result;
                var user = JArray.Parse(responseString);

                return user.Count > 0 ? new Guid(user[0]["id"]!.ToString()) : Guid.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Guid> GetUserByUserId(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"admin/realms/auth-demo/users?userid={userId}");
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new UserNotFoundException("User not found");
                    }
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new KeycloakException("Error on "
                            + _httpClient.BaseAddress
                            + "get-user-by-username"
                            + "Status Code: "
                            + response.StatusCode
                            + "Content : "
                            + response.Content);
                }

                var responseString = response.Content.ReadAsStringAsync().Result;
                var user = JArray.Parse(responseString);

                return user.Count > 0 ? new Guid(user[0]["id"]!.ToString()) : Guid.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateUser(Guid id, UpdateUserDto user)
        {
            try
            {
                var userData = new
                {
                    email = user.UserEmail,
                    firstName = user.UserName,
                    lastName = user.UserLastName,
                    attributes = new Dictionary<string, object>
                    {
                        { "phone", new[] { user.UserPhone ?? "" } },
                        { "address", new[] { user.UserAddress ?? "" } }
                    },
                    enabled = true
                };

                // Serializar el objeto a JSON
                var jsonBody = JsonSerializer.Serialize(userData);
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"admin/realms/auth-demo/users/{id}", contentJson);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new UserNotFoundException("User not found");
                    }
                    else if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        throw new UserExistException("User already exists, try with another username or email");
                    }
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new KeycloakException("Error on "
                            + _httpClient.BaseAddress
                            + "update-user"
                            + "Status Code: "
                            + response.StatusCode
                            + "Content : "
                            + response.Content);
                }
                var content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentNullException(nameof(userEmail), "El userEmail no puede ser nulo o vacío.");
            }

            // 🔹 Usar `GetUserByUserName` para obtener el `userId` a partir del email
            var userId = await GetUserByUserName(userEmail);

            if (userId == Guid.Empty)
            {
                throw new KeycloakException("No se encontró ningún usuario con el email proporcionado.");
            }

            // 🔹 Enviar el correo de restablecimiento de contraseña con `userId`
            var actions = new[] { "UPDATE_PASSWORD" };
            var jsonBody = JsonSerializer.Serialize(actions);
            var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"admin/realms/auth-demo/users/{userId}/execute-actions-email", contentJson);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error en la solicitud: {response.StatusCode} - {errorMessage}");
            }

            return true;
        }


        public async Task<bool> ChangeUserPasswordSecureAsync(string username, string currentPassword, string newPassword)
        {
            try
            {
                // 🔹 Obtener el userId por username
                var userId = await GetUserByUserName(username);

                if (userId == Guid.Empty)
                {
                    Console.WriteLine("No se encontró ningún usuario con el email proporcionado.");
                    return false;
                }

                var token = LoginAsync(username, currentPassword);

                if (string.IsNullOrWhiteSpace(token.Result))
                {
                    Console.WriteLine("Credenciales incorrectas.");
                    return false;
                }


                // 🔁 Cambiar contraseña
                var passwordData = new
                {
                    type = "password",
                    value = newPassword,
                    temporary = false
                };

                var jsonBody = JsonSerializer.Serialize(passwordData);
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"admin/realms/auth-demo/users/{userId}/reset-password", contentJson);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error al cambiar la contraseña: " + response.StatusCode);
                    return false;
                }

                Console.WriteLine("Contraseña cambiada correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return false;
            }
        }
    }
}
