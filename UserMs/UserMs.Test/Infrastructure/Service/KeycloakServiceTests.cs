using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Moq.Protected;
using UserMs.Commoon.Dtos;
using UserMs.Infrastructure;
using UserMs.Infrastructure.Service.Keycloak;
using Xunit;
using AuthMs.Common.Exceptions;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Test.Infrastructure.Service
{
    public class KeycloakServiceTests
    {
       /* private HttpClient _httpClient;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<IOptions<HttpClientUrl>> _httpClientUrlMock;
        private KeycloakService _keycloakService;

        public KeycloakServiceTests()
        {
            // 🔹 Simulación de `IHttpContextAccessor`
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = "Bearer mock-token";
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(mockHttpContext);

            // 🔹 Simulación de `HttpClientUrl`
            _httpClientUrlMock = new Mock<IOptions<HttpClientUrl>>();
            _httpClientUrlMock.Setup(x => x.Value).Returns(new HttpClientUrl { ApiUrl = "http://localhost:18080" });

            // 🔹 Inicialización de `HttpClient` con `MockHttpMessageHandler`
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"access_token\": \"mock-token\" }")
            };
            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);

            // 🔹 Inicialización de `KeycloakService`
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);
        }

        //Get Token Administrator

        [Fact]
        public async Task GetTokenAdministrator_ShouldReturnValidToken()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"access_token\": \"admin-token\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var token = await _keycloakService.GetTokenAdministrator();

            Assert.NotNull(token);
            Assert.Equal("admin-token", token);
        }

        //Pruebad e Client Roles Id
        [Fact]

        public async Task GetClientRolesId_ShouldReturnRoleId_WhenRoleExists()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{ \"id\": \"role123\", \"name\": \"Administrator\" }]")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.GetClientRolesId("client123", "user123", "Administrator");

            Assert.Equal("role123", result);
        }

        [Fact]
        public async Task GetClientRolesId_ShouldReturnEmptyString_WhenRoleDoesNotExist()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{ \"id\": \"role456\", \"name\": \"AnotherRole\" }]") // 🔹 Simula que el rol no está en la lista
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.GetClientRolesId("client123", "user123", "Administrator");

            Assert.Equal("", result);
        }

        [Fact]
        public async Task GetClientRolesId_ShouldThrowException_WhenUserNotFound()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"User not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<UserNotFoundException>(() => _keycloakService.GetClientRolesId("client123", "unknownUser", "Administrator"));
        }

        [Fact]
        public async Task GetClientRolesId_ShouldThrowException_WhenServerErrorOccurs()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.GetClientRolesId("client123", "user123", "Administrator"));
        }

        [Fact]
        public async Task GetClientRolesId_ShouldThrowException_WhenResponseIsInvalidJson()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid_json") // 🔹 Simula respuesta malformada
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<JsonReaderException>(() => _keycloakService.GetClientRolesId("client123", "user123", "Administrator"));
        }


        //Get Client Id 

        [Fact]
        public async Task GetClientId_ShouldReturnClientId_WhenValidResponse()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{ \"clientId\": \"admin-client\", \"id\": \"client123\" }]")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(fakeHandler);
            var keycloakService = new KeycloakService(httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var clientId = await keycloakService.GetClientId("admin-client");

            Assert.Equal("client123", clientId);
        }

        [Fact]
        public async Task GetClientId_ShouldReturnEmptyString_WhenClientNotFound()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{ \"clientId\": \"another-client\", \"id\": \"clientXYZ\" }]")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(fakeHandler);
            var keycloakService = new KeycloakService(httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var clientId = await keycloakService.GetClientId("nonexistent-client");

            Assert.Equal("", clientId); // 🔹 Verifica que retorna cadena vacía si el cliente no existe
        }

        [Fact]
        public async Task GetClientId_ShouldThrowException_WhenResponseIsEmpty()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(fakeHandler);
            var keycloakService = new KeycloakService(httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<JsonReaderException>(() => keycloakService.GetClientId("admin-client"));
        }

        [Fact]
        public async Task GetClientId_ShouldThrowException_WhenResponseIsNotSuccessful()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(fakeHandler);
            var keycloakService = new KeycloakService(httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => keycloakService.GetClientId("admin-client"));
        }


       //Pruebas de buscar por user Name

        [Fact]
        public async Task GetUserByUserName_ShouldReturnUserGuid()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{ \"id\": \"123e4567-e89b-12d3-a456-426614174000\" }]")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var userId = await _keycloakService.GetUserByUserName("testuser");

            Assert.Equal(Guid.Parse("123e4567-e89b-12d3-a456-426614174000"), userId);
        }

        [Fact]
        public async Task GetUserByUserName_ShouldReturnEmptyGuid_WhenUserDoesNotExist()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]") // 🔹 Simula usuario no encontrado
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.GetUserByUserName("nonexistentuser");

            Assert.Equal(Guid.Empty, result);
        }

        [Fact]
        public async Task GetUserByUserName_ShouldThrowException_WhenUserNotFound()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"User not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<UserNotFoundException>(() => _keycloakService.GetUserByUserName("unknownuser"));
        }

        [Fact]
        public async Task GetUserByUserName_ShouldThrowException_WhenServerErrorOccurs()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.GetUserByUserName("erroruser"));
        }

        [Fact]
        public async Task GetUserByUserName_ShouldThrowException_WhenResponseIsInvalidJson()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid_json") // 🔹 Simula JSON inválido
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<JsonReaderException>(() => _keycloakService.GetUserByUserName("jsonerroruser"));
        }

        //Pruebas de crear usuario 

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUserSuccessfully()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Headers = { Location = new Uri("http://localhost:18080/admin/realms/auth-demo/users/user123") }
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.CreateUserAsync("user@example.com", "Pass123!", "John", "Doe", "1234567890", "My Address");

            Assert.Contains("Usuario creado y enlace de activación enviado con éxito.", result);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenUserAlreadyExists()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent("{ \"error\": \"User already exists\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<UserExistException>(() => _keycloakService.CreateUserAsync("existing@example.com", "Pass123!", "Jane", "Doe", "9876543210", "Another Address"));
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenServerErrorOccurs()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.CreateUserAsync("servererror@example.com", "Pass123!", "Error", "Test", "0000000000", "Error Address"));
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenResponseHasInvalidJson()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("invalid_json")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<NullReferenceException>(() => _keycloakService.CreateUserAsync("jsonerror@example.com", "Pass123!", "JSON", "Error", "9999999999", "JSON Address"));
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenUserIdCannotBeExtracted()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Headers = { Location = null } // 🔹 Simula que la ubicación no está en los headers
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<NullReferenceException>(() => _keycloakService.CreateUserAsync("nouserid@example.com", "Pass123!", "NoUser", "ID", "1111111111", "No ID Address"));
        }

        //Eliminar Usuario

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUserSuccessfully()
        {
            var userId = Guid.NewGuid();

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NoContent); // 🔹 Simula eliminación exitosa

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.DeleteUserAsync(userId);

            Assert.NotNull(result);
            Assert.Equal("", result);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldThrowException_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"User not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<UserNotFoundException>(() => _keycloakService.DeleteUserAsync(userId));
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldThrowException_WhenServerErrorOccurs()
        {
            var userId = Guid.NewGuid();

            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.DeleteUserAsync(userId));
        }

        //Bucar usuario por id

        [Fact]
        public async Task GetUserByUserId_ShouldReturnUserId_WhenUserExists()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{ \"id\": \"b8239bba-6f83-411f-8f71-e1263cfd07d1\" }]")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.GetUserByUserId("b8239bba-6f83-411f-8f71-e1263cfd07d1");

            Assert.NotEqual(Guid.Empty, result);
        }

        [Fact]
        public async Task GetUserByUserId_ShouldReturnEmptyGuid_WhenUserDoesNotExist()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]") // 🔹 Simula usuario no encontrado
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.GetUserByUserId("nonexistentuserid");

            Assert.Equal(Guid.Empty, result);
        }

        [Fact]
        public async Task GetUserByUserId_ShouldThrowException_WhenUserNotFound()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"User not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<UserNotFoundException>(() => _keycloakService.GetUserByUserId("unknownuserid"));
        }

        [Fact]
        public async Task GetUserByUserId_ShouldThrowException_WhenServerErrorOccurs()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.GetUserByUserId("erroruserid"));
        }

        [Fact]
        public async Task GetUserByUserId_ShouldThrowException_WhenResponseIsInvalidJson()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid_json") // 🔹 Simula JSON inválido
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<JsonReaderException>(() => _keycloakService.GetUserByUserId("jsonerroruserid"));
        }
        //Pruebas de enviar el enlace 

        [Fact]
        public async Task SendActivationEmail_ShouldSendSuccessfully()
        {
            var userId = "user123";
            var userEmail = "user@example.com";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK); // 🔹 Simula envío exitoso

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.SendActivationEmail(userId, userEmail);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task SendActivationEmail_ShouldThrowException_WhenUserNotFound()
        {
            var userId = "invalidUser";
            var userEmail = "invalid@example.com";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"User not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<Exception>(() => _keycloakService.SendActivationEmail(userId, userEmail));
        }

        [Fact]
        public async Task SendActivationEmail_ShouldThrowException_WhenServerErrorOccurs()
        {
            var userId = "errorUser";
            var userEmail = "servererror@example.com";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<Exception>(() => _keycloakService.SendActivationEmail(userId, userEmail));
        }

        

        [Fact]
        public async Task SendActivationEmail_ShouldThrowException_WhenBadRequestOccurs()
        {
            var userId = "badRequestUser";
            var userEmail = "badrequest@example.com";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{ \"error\": \"Invalid request format\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<Exception>(() => _keycloakService.SendActivationEmail(userId, userEmail));
        }

        //Prueba de Get Token

        [Fact]
        public async Task GetTokenAsync_ShouldReturnAccessToken_WhenSuccessful()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"access_token\": \"mock-token\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.GetTokenAsync();

            Assert.NotNull(result);
            Assert.Contains("access_token", result);
        }

        [Fact]
        public async Task GetTokenAsync_ShouldThrowException_WhenUnauthorized()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{ \"error\": \"invalid_client\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => _keycloakService.GetTokenAsync());
        }

        [Fact]
        public async Task GetTokenAsync_ShouldThrowException_WhenServerErrorOccurs()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"server_error\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => _keycloakService.GetTokenAsync());
        }
        //Remove 

        [Fact]
        public async Task RemoveClientRoleFromUser_ShouldRemoveRoleSuccessfully()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]") // 🔹 Simula respuesta vacía sin error
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await _keycloakService.RemoveClientRoleFromUser(userId, roleName);
        }

        [Fact]
        public async Task RemoveClientRoleFromUser_ShouldThrowException_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"User not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.RemoveClientRoleFromUser(userId, roleName));
        }

        [Fact]
        public async Task RemoveClientRoleFromUser_ShouldThrowException_WhenRoleNotFound()
        {
            var userId = Guid.NewGuid();
            var roleName = "RolInexistente";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"Role not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.RemoveClientRoleFromUser(userId, roleName));
        }

        [Fact]
        public async Task RemoveClientRoleFromUser_ShouldThrowException_WhenServerErrorOccurs()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.RemoveClientRoleFromUser(userId, roleName));
        }

        [Fact]
        public async Task RemoveClientRoleFromUser_ShouldThrowException_WhenResponseIsInvalidJson()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid_json") // 🔹 Simula respuesta malformada
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.RemoveClientRoleFromUser(userId, roleName));
        }



        //Pruebas Asiganr rol al cliente 

        [Fact]
        public async Task AssignClientRoleToUser_ShouldAssignRoleSuccessfully()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]") // 🔹 Simula respuesta vacía sin error
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await _keycloakService.AssignClientRoleToUser(userId, roleName);
        }
        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowException_WhenClientIdIsInvalid()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"Client ID not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));
        }

        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowException_WhenUnauthorizedAccess()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{ \"error\": \"Unauthorized access\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));
        }

        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowException_WhenForbidden()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("{ \"error\": \"Access forbidden\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));
        }

        /*[Fact]
        public async Task AssignClientRoleToUser_ShouldThrowException_WhenTimeoutOccurs()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var fakeHandler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK), simulateTimeout: true);
            _httpClient = new HttpClient(fakeHandler)
            {
                Timeout = TimeSpan.FromSeconds(2) // 🔹 Simula timeout
            };
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<TaskCanceledException>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));
        }
        
        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowException_WhenResponseIsInvalidJson()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid_json") // 🔹 Simula respuesta malformada
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<Exception>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));
        }

        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowException_WhenUserAlreadyHasRole()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{ \"name\": \"Administrator\" }]") // 🔹 Simula que el usuario ya tiene el rol asignado
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<Exception>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));
        }

        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowException_WhenRoleNotFound()
        {
            var userId = Guid.NewGuid();
            var roleName = "RolInexistente"; // 🔹 Un rol que no existe en Keycloak

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"Role not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));
        }

        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowException_WhenServerErrorOccurs()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));
        }

        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowKeycloakException_WhenClientIdIsInvalid()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"Client ID not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var exception = await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));

            Assert.Contains("Error en Keycloak: Client not found", exception.Message);
        }

        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowKeycloakException_WhenUnauthorizedAccess()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{ \"error\": \"Unauthorized access\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var exception = await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));

            Assert.Contains("Error en Keycloak: Error on", exception.Message);
        }

        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowInvalidOperationException_WhenRoleAlreadyAssigned()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{ \"name\": \"Administrator\" }]") // 🔹 Simula que el usuario ya tiene el rol
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));

            Assert.Contains("Error crítico en AssignClientRoleToUser", exception.Message);
        }

        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowJsonException_WhenResponseIsInvalidJson()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid_json") // 🔹 Simula respuesta malformada
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));

            Assert.Contains("Error crítico en AssignClientRoleToUser", exception.Message);
        }

        [Fact]
        public async Task AssignClientRoleToUser_ShouldThrowTaskCanceledException_WhenTimeoutOccurs()
        {
            var userId = Guid.NewGuid();
            var roleName = "Administrador";

            var fakeHandler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK), simulateTimeout: true);
            _httpClient = new HttpClient(fakeHandler)
            {
                Timeout = TimeSpan.FromSeconds(2) // 🔹 Simula timeout correctamente
            };

            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() => _keycloakService.AssignClientRoleToUser(userId, roleName));

            Assert.Contains("Error crítico en AssignClientRoleToUser: The request was canceled due to the configured HttpClient.Timeout of 2 seconds elapsing", exception.Message);
        }

        //Pruebas de actualización de usuario
        [Fact]
        public async Task UpdateUser_ShouldUpdateSuccessfully()
        {
            var userDto = new UpdateUserDto { UserEmail = "new@mail.com", UserName = "UpdatedUser" };
            var userId = Guid.NewGuid();

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await _keycloakService.UpdateUser(userId, userDto);

            Assert.True(true); // Verifica que no lanza excepción
        }

        [Fact]
        public async Task UpdateUser_ShouldThrowException_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var updateUserDto = new UpdateUserDto
            {
                UserEmail = "notfound.email@example.com",
                UserName = "NonExistentUser",
                UserLastName = "DoesNotExist"
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{ \"error\": \"User not found\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<UserNotFoundException>(() => _keycloakService.UpdateUser(userId, updateUserDto));
        }

        [Fact]
        public async Task UpdateUser_ShouldThrowException_WhenUserAlreadyExists()
        {
            var userId = Guid.NewGuid();
            var updateUserDto = new UpdateUserDto
            {
                UserEmail = "existing.email@example.com",
                UserName = "ExistingUser"
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent("{ \"error\": \"User already exists\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<UserExistException>(() => _keycloakService.UpdateUser(userId, updateUserDto));
        }

        [Fact]
        public async Task UpdateUser_ShouldThrowException_WhenServerError()
        {
            var userId = Guid.NewGuid();
            var updateUserDto = new UpdateUserDto
            {
                UserEmail = "error.email@example.com",
                UserName = "ServerErrorUser"
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<KeycloakException>(() => _keycloakService.UpdateUser(userId, updateUserDto));
        }





        //Pruebas de login 

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenValidCredentials()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"access_token\": \"mock-token\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var token = await _keycloakService.LoginAsync("validUser", "validPass");

            Assert.NotNull(token);
            Assert.Contains("mock-token", token);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenInvalidCredentials()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{ \"error\": \"Invalid username or password\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => _keycloakService.LoginAsync("invalidUser", "wrongPass"));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenServerError()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => _keycloakService.LoginAsync("user123", "password123"));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenEmptyUsernameOrPassword()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"access_token\": \"mock-token\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => _keycloakService.LoginAsync("", "validPass"));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _keycloakService.LoginAsync("validUser", ""));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenTimeoutOccurs()
        {
            var fakeHandler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK), simulateTimeout: true);
            _httpClient = new HttpClient(fakeHandler)
            {
                Timeout = TimeSpan.FromSeconds(2) // 🔹 Simula timeout
            };

            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<TaskCanceledException>(() => _keycloakService.LoginAsync("user123", "password123"));
        }

        //Pruebas par LogOut 

        [Fact]
        public async Task LogOutAsync_ShouldReturnEmptyString_WhenSuccessful()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _httpContextAccessorMock.Setup(x => x.HttpContext!.Request.Headers["Authorization"])
                .Returns("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyMTIzIiwiYXpwIjoiY2xpZW50LTEyMyJ9.abc123"); // 🔹 Simula JWT válido

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.LogOutAsync();

            Assert.NotNull(result);
            Assert.Equal("", result);
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenUnauthorized()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{ \"error\": \"Unauthorized access\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenServerError()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{ \"error\": \"Server failure\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenTokenIsInvalid()
        {
            _httpContextAccessorMock.Setup(x => x.HttpContext!.Request.Headers["Authorization"])
                .Returns("Bearer invalid-token");

            var fakeHandler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenTimeoutOccurs()
        {
            var fakeHandler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK), simulateTimeout: true);
            _httpClient = new HttpClient(fakeHandler)
            {
                Timeout = TimeSpan.FromSeconds(2) // 🔹 Simula timeout
            };

            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenServiceUnavailable()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("{ \"error\": \"Service unavailable\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenForbidden()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("{ \"error\": \"Access forbidden\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenResponseHasInvalidJson()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid_json") // 🔹 Simula respuesta con JSON malformado
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenBadRequestOccurs()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{ \"error\": \"Invalid request format\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenTooManyRequests()
        {
            var mockResponse = new HttpResponseMessage((HttpStatusCode)429) // 🔹 Simula error 429 (Too Many Requests)
            {
                Content = new StringContent("{ \"error\": \"Rate limit exceeded\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenGatewayTimeoutOccurs()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
            {
                Content = new StringContent("{ \"error\": \"Gateway timeout\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }

        [Fact]
        public async Task LogOutAsync_ShouldThrowException_WhenResponseIsEmpty()
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("") // 🔹 Simula respuesta vacía
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _keycloakService.LogOutAsync());
        }


        // Pruebas para `SendPasswordResetEmailAsync`


        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldReturnTrue_WhenSuccessful()
        {
            var userId = "user123";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK); // 🔹 Simula éxito
            var fakeHandler = new MockHttpMessageHandler(mockResponse);

            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            var result = await _keycloakService.SendPasswordResetEmailAsync(userId);

            Assert.True(result); // 🔹 Verifica que devuelve `true`
        }


        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldThrowException_WhenForbidden()
        {
            var userId = "user123";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("{ \"error\": \"Forbidden access\" }") // 🔹 Simula error en el servidor
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => _keycloakService.SendPasswordResetEmailAsync(userId));
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldThrowException_WhenTimeoutOccurs()
        {
            var userId = "user123";
            var fakeHandler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK), simulateTimeout: true);

            _httpClient = new HttpClient(fakeHandler)
            {
                Timeout = TimeSpan.FromSeconds(2) // 🔹 Tiempo límite más corto que el retraso de 5s
            };

            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            //using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)); // 🔹 Cancela después de 2s

            await Assert.ThrowsAsync<TaskCanceledException>(() => _keycloakService.SendPasswordResetEmailAsync(userId));
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldThrowException_WhenBadRequestOccurs()
        {
            var userId = "user123";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{ \"error\": \"Invalid request format\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => _keycloakService.SendPasswordResetEmailAsync(userId));
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldThrowException_WhenTooManyRequests()
        {
            var userId = "user123";
            var mockResponse = new HttpResponseMessage((HttpStatusCode)429) // 🔹 Simula error 429 (Too Many Requests)
            {
                Content = new StringContent("{ \"error\": \"Rate limit exceeded\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => _keycloakService.SendPasswordResetEmailAsync(userId));
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldThrowException_WhenServiceUnavailable()
        {
            var userId = "user123";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("{ \"error\": \"Service unavailable\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => _keycloakService.SendPasswordResetEmailAsync(userId));
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldThrowException_WhenGatewayTimeoutOccurs()
        {
            var userId = "user123";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
            {
                Content = new StringContent("{ \"error\": \"Gateway timeout\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => _keycloakService.SendPasswordResetEmailAsync(userId));
        }
        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldThrowException_WhenUserIdIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _keycloakService.SendPasswordResetEmailAsync(null));
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldThrowException_WhenUserIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _keycloakService.SendPasswordResetEmailAsync(""));
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldThrowException_WhenUserIdIsWhitespace()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _keycloakService.SendPasswordResetEmailAsync("   "));
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldThrowException_WhenUnauthorized()
        {
            var userId = "user123";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{ \"error\": \"Unauthorized access\" }")
            };

            var fakeHandler = new MockHttpMessageHandler(mockResponse);
            _httpClient = new HttpClient(fakeHandler);
            _keycloakService = new KeycloakService(_httpClient, _httpContextAccessorMock.Object, _httpClientUrlMock.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => _keycloakService.SendPasswordResetEmailAsync(userId));
        }

        

       
      */


    }
}
