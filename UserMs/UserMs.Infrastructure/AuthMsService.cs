﻿using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using UserMs.Core.Interface;
using UserMs.Domain.Entities;
using Microsoft.AspNetCore.Http;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Infrastructure
{
    public class AuthMsService : IAuthMsService
    {
        public readonly HttpClient _httpClient;
        public readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _httpClientUrl;
        public AuthMsService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptions<HttpClientUrl> httpClientUrl)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _httpClientUrl = httpClientUrl.Value.ApiUrl;

            var headerToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
            _httpClient.BaseAddress = new Uri(_httpClientUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {headerToken}");

        }

        public async Task AssignClientRoleToUser(Guid id, string roleName)
        {
            try
            {
                var role = String.Empty;
                if (roleName == "Administrador")
                {
                    role = "Administrator";
                }
                else if (roleName == "Subastador")
                {
                    role = "Auctioneer";
                }
                else if (roleName == "Postor")
                {
                    role = "Bidder";
                }
                else
                {
                    role = "Support";
                }
                string ClientId = (roleName == "Conductor") ? "mobileclient" : "webclient"; // Que hace esto
                var userData = new
                {
                    userId = id,
                    roleName = role,
                    clientId = ClientId
                };
                Console.WriteLine("UserData: " + userData);
                var jsonBody = JsonSerializer.Serialize(userData);
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"auth-demo/assingRole", contentJson);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new AuthenticationMsException("Error on "
                            + _httpClient.BaseAddress
                            + "assing-role-user"
                            + "Status Code: "
                            + response.StatusCode
                            + "Content : "
                            + response.Content);
                }
                var content = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> CreateUserAsync(string userEmail, string userPassword)
        {
            try
            {
                var userData = new
                {
                    //id = user.UserId.Value,
                    username = userEmail,
                    email = userEmail,
                    password = userPassword
                };
                // Serializar el objeto a JSON
                var jsonBody = JsonSerializer.Serialize(userData);
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("auth-demo/createUser", contentJson);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new AuthenticationMsException("Error on "
                            + _httpClient.BaseAddress
                            + "create-user "
                            + "Status Code: "
                            + response.StatusCode
                            + "Content : "
                            + response.Content);
                }
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> DeleteUserAsync(UserId userId)
        {
            try
            {
                Console.WriteLine("UserId: " + userId.Value);
                var response = await _httpClient.DeleteAsync($"auth-demo/deleteUser/{userId.Value}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new AuthenticationMsException("Error on "
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
            catch
            {
                throw;
            }
        }

        public async Task<Guid> GetUserByUserName(UserEmail userName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"auth-demo/{userName.Value}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new AuthenticationMsException("Error on "
                            + _httpClient.BaseAddress
                            + "get-user-by-username"
                            + "Status Code: "
                            + response.StatusCode
                            + "Content : "
                            + response.Content);
                }

                var responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("ResponseString: " + responseString);
                return new Guid(responseString.Replace("\"", ""));
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateUser(UserId id, Base user)
        {
            try
            {
                var userData = new
                {
                    email = user.UserEmail.Value
                };


                var jsonBody = JsonSerializer.Serialize(userData);
                var contentJson = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"auth-demo/{id.Value}", contentJson);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new AuthenticationMsException("Error on "
                            + _httpClient.BaseAddress
                            + "update-user"
                            + "Status Code: "
                            + response.StatusCode
                            + "Content : "
                            + response.Content);
                }
                var content = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                throw;
            }
        }
    }
}
