
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

namespace UserMs.Infrastructure.Service.Keycloak
{
    public class KeycloakService : IKeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
       private readonly IEmailSender _emailSender;
        private readonly string _httpClientUrl;
        private readonly string _realm = "auth-demo";
        private readonly string _baseUrl = "http://localhost:18080";
        private readonly string _adminUsername = "test@test.com";
        private readonly string _adminPassword = "1234";



        public KeycloakService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptions<HttpClientUrl> httpClientUrl, IEmailSender emailSender)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _httpClientUrl = httpClientUrl.Value.ApiUrl;

            //* Configuracion del HttpClient
            var headerToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
            _httpClient.BaseAddress = new Uri(_httpClientUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {headerToken}");

            _emailSender = new EmailSender();

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


        public async Task<string> CreateUserAsync(string userEmail, string userPassword, string userName, string userLastName, string userPhone, string userAddress)
        {
            try
            {
                // Crear el usuario en Keycloak con atributos personalizados
                var userData = new
                {
                    username = userEmail,
                    email = userEmail,
                    firstName = userName,  // Nombre del usuario
                    lastName = userLastName,  // Apellido del usuario
                    emailVerified = true,  // Confirmación de correo
                    enabled = true,  // Habilitar usuario
                    credentials = new[]
                    {
                new
                {
                    type = "password",
                    value = $"{userPassword}",
                    temporary = false  // Contraseña no temporal
                }
            },
                    attributes = new Dictionary<string, object>
                    {
                        { "phone", new[] { userPhone ?? "" } },
                        { "address", new[] { userAddress ?? "" } }
                    }
                };

                // Serializar el objeto a JSON
                var jsonBody = JsonSerializer.Serialize(userData);
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // Enviar solicitud para crear el usuario
                var createUserResponse = await _httpClient.PostAsync("http://localhost:18080/admin/realms/auth-demo/users", contentJson);

                if (!createUserResponse.IsSuccessStatusCode)
                {
                    if (createUserResponse.StatusCode == HttpStatusCode.Conflict)
                    {
                        throw new UserExistException("El usuario ya existe. Intenta con otro correo.");
                    }
                    else
                    {
                        throw new KeycloakException($"Error creando usuario: {createUserResponse.StatusCode}");
                    }
                }

                // Obtener el userId del usuario creado
                var createdUserLocation = createUserResponse.Headers.Location.ToString();
                var userId = createdUserLocation.Split('/').Last();
                Console.WriteLine($"Created User Location: {userId}");

                // Enviar enlace de activación por correo
                var activationResponse = await SendActivationEmail(userId, userEmail);



                return "Usuario creado y enlace de activación enviado con éxito.";
            }
            catch (Exception ex)
            {
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

                List<dynamic> assignedRoles;
                try
                {
                    assignedRoles = JsonSerializer.Deserialize<List<dynamic>>(responseString) ?? new List<dynamic>();
                }
                catch (JsonException ex)
                {
                    throw new KeycloakException($"Error procesando JSON: {ex.Message}");
                }

                if (assignedRoles.Any(r => r.name == role))
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

        public async Task RemoveClientRoleFromUser(Guid userId, string roleName)
        {
            try
            {
                var role = roleName switch
                {
                    "Administrador" => "Administrator",
                    "Subastador" => "Auctioneer",
                    "Postor" => "Bidder",
                    _ => "Support"
                };

                var clientId = await GetClientId("admin-client");
                var roleId = await GetClientRolesId(clientId, userId.ToString(), role);

                var roles = new[] { new { id = roleId, name = role } };

                var jsonBody = JsonSerializer.Serialize(roles);
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.DeleteAsync($"admin/realms/auth-demo/users/{userId}/role-mappings/clients/{clientId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new KeycloakException($"Error eliminando rol en Keycloak: {response.StatusCode} - {errorContent}");
                }

                Console.WriteLine($"Rol '{roleName}' eliminado del usuario {userId} en Keycloak.");
            }
            catch (KeycloakException ex)
            {
                
                throw; // 🔹 Relanza la excepción correctamente sin envolverla en AggregateException
            }
            catch (Exception ex)
            {
               
                throw new KeycloakException($"Error crítico: {ex.Message}");
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
    }
}
