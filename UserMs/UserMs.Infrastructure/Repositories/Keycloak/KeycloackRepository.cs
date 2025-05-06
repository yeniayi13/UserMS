
using AuthMs.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using UserMs.Core;
using UserMs.Commoon.Dtos;
using UserMs.Infrastructure.Exceptions;
using UserMs.Commoon.Dtos.Users.Request;
using UserMs.Infrastructure;

namespace AuthMs.Infrastructure
{
    public class KeycloakRepository : IKeycloakRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EmailSender _emailSender;
        private readonly string _httpClientUrl;


        public KeycloakRepository(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptions<HttpClientUrl> httpClientUrl)
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
        /*public async Task<string> GetTokenAsync()
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

        }*/

        public async Task<string> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsync("realms/auth-demo/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                 { "grant_type", "password" },
                 { "client_id", "confidential-client" },
                 { "username", username },
                 {"password", password},
                //{"client_secret", "REdOcKznwuvtZ54jVVt9ebc3nCz6uqMy" }
            }));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            //Parse the token from the response
            return content;
        }

        public async Task<string> LogOutAsync()
        {
            //* MANEJA EL TOKEN DEL HEADER
            var headerToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
            var accessToken = new JwtSecurityTokenHandler().ReadToken(headerToken) as JwtSecurityToken;
            var userId = accessToken!.Payload["sub"];
            var client = accessToken!.Payload["azp"];

            // _httpClient.BaseAddress = new Uri("http://localhost:18080/");
            // _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");

            var response = await _httpClient.PostAsync($"admin/realms/auth-demo/users/{userId}/logout",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                // {"refresh_token", refreshToken },
                // { "client_id", $"{client}" },
                //{"client_secret", "REdOcKznwuvtZ54jVVt9ebc3nCz6uqMy" }
            }));

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return content;

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
            var requestBody = JsonSerializer.Serialize(new[] { "VERIFY_EMAIL" }); // Acción de activación
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            //var requestUrl = $"http://localhost:18080/admin/realms/auth-demo/users/{userId}/execute-actions-email";
          
            var response = await _httpClient.PutAsync($"http://localhost:18080/admin/realms/auth-demo/users/{userId}/execute-actions-email", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error enviando el correo de activación: {response.StatusCode}");
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

        public async Task AssignClientRoleToUser(Guid userId, string clientId, string roleName)
        {
            try
            {
                //* La Client Id es un guid
                //var ClientId = "f49adac8-b4f4-4791-9266-9d097c5875ff";
                var ClientId = this.GetClientId(clientId).Result.ToString();

                var rolId = this.GetClientRolesId(ClientId, userId.ToString(), roleName).Result;
                var roles = new[] { new { id = rolId, name = roleName } };
                var userData = new { };

                // Serializar el objeto a JSON
                var jsonBody = JsonSerializer.Serialize(roles);
                // Crear el contenido de la solicitud con el encabezado Content-Type
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"admin/realms/auth-demo/users/{userId}/role-mappings/clients/{ClientId}", contentJson);

                if (!response.IsSuccessStatusCode)
                {

                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new KeycloakException("Error on "
                            + _httpClient.BaseAddress
                            + " assing-role-user"
                            + " Status Code: "
                            + response.StatusCode
                            + " Content : "
                            + response.Content);
                }
                var content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw ex;
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

    }
}
