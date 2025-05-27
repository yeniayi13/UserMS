using System;
using System.Data.Entity.Core.Objects;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserMs.Commoon.Dtos.Keycloak;
using UserMs.Commoon.Dtos;
using Xunit;
using MediatR;
using UserMs.Application.Commands.Keycloak;
using UserMs.Controllers;
using ObjectResult = Microsoft.AspNetCore.Mvc.ObjectResult;

namespace UserMs.Test.Controller
{
   
    public class AuthControllerTests
    {
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly Mock<IMediator> _mockMediator;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _mockLogger = new Mock<ILogger<AuthController>>();
            _mockMediator = new Mock<IMediator>();
            _authController = new AuthController(_mockLogger.Object, _mockMediator.Object);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            var mockMediator = new Mock<IMediator>();

            var exception = Assert.Throws<ArgumentNullException>(() => new AuthController(null, mockMediator.Object));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMediatorIsNull()
        {
            var mockLogger = new Mock<ILogger<AuthController>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new AuthController(mockLogger.Object, null));

            Assert.Equal("mediator", exception.ParamName);
        }

        [Fact]
        public void Constructor_CreatesInstance_WhenValidParametersAreProvided()
        {
            var mockLogger = new Mock<ILogger<AuthController>>();
            var mockMediator = new Mock<IMediator>();

            var controller = new AuthController(mockLogger.Object, mockMediator.Object);

            Assert.NotNull(controller);
        }


        [Fact]
        public async Task LoginAsync_ReturnsOk_WhenValidRequest()
        {
            var loginDto = new LoginDto { Username= "test@example.com", Password = "password123" };
            var token = "fakeToken";

            _mockMediator.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
                         .ReturnsAsync(token);

            var result = await _authController.LoginAsync(loginDto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(token, result.Value);
        }

        [Fact]
        public async Task LoginAsync_ReturnsBadRequest_WhenRequestIsNull()
        {
            var result = await _authController.LoginAsync(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El cuerpo de la solicitud no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            var loginDto = new LoginDto { Username = "wrong@example.com", Password = "wrongpassword" };

            _mockMediator.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
                         .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _authController.LoginAsync(loginDto) as UnauthorizedObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
            Assert.Equal("Credenciales incorrectas.", result.Value);
        }

        [Fact]
        public async Task LogOutAsync_ReturnsOk_WhenSuccessful()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<LogOutCommand>(), default))
                         .ReturnsAsync("true");

            var result = await _authController.LogOutAsync() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Sesión cerrada correctamente.", result.Value);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsOk_WhenSuccessful()
        {
            var resetPasswordDto = new ResetPasswordDto { UserEmail = "test@example.com" };

            _mockMediator.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), default))
                         .ReturnsAsync(true);

            var result = await _authController.ResetPasswordAsync(resetPasswordDto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Correo de recuperación enviado exitosamente.", result.Value);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsBadRequest_WhenEmailIsEmpty()
        {
            var resetPasswordDto = new ResetPasswordDto { UserEmail = "" };

            var result = await _authController.ResetPasswordAsync(resetPasswordDto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El correo electrónico no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsNotFound_WhenUserNotFound()
        {
            var resetPasswordDto = new ResetPasswordDto { UserEmail = "notfound@example.com" };

            _mockMediator.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), default))
                         .ThrowsAsync(new KeyNotFoundException());

            var result = await _authController.ResetPasswordAsync(resetPasswordDto) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("El correo electrónico ingresado no está registrado.", result.Value);
        }

        [Fact]
        public async Task LoginAsync_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            var loginDto = new LoginDto { Username = "test@example.com", Password = "password123" };

            _mockMediator.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _authController.LoginAsync(loginDto) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al procesar el inicio de sesión.", result.Value);
        }

        [Fact]
        public async Task LogOutAsync_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<LogOutCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _authController.LogOutAsync() as Microsoft.AspNetCore.Mvc.ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al cerrar sesión.", result.Value);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            var resetPasswordDto = new ResetPasswordDto { UserEmail = "test@example.com" };

            _mockMediator.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _authController.ResetPasswordAsync(resetPasswordDto) as Microsoft.AspNetCore.Mvc.ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al procesar el restablecimiento de contraseña.", result.Value);
        }
    }
}
