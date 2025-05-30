using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserMs.Application.Commands.RolesPermission;
using UserMs.Application.Commands.UsersRoles;
using UserMs.Application.Queries.Permission;
using UserMs.Application.Queries.Roles_Permission;
using UserMs.Application.Queries.Roles;
using UserMs.Application.Queries.User_Roles;
using UserMs.Commoon.Dtos.Users.Request.RolePermission;
using UserMs.Commoon.Dtos.Users.Request.UserRole;
using UserMs.Commoon.Dtos.Users.Response.Permission;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Controllers;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using Xunit;

namespace UserMs.Test.Controller
{

    public class RoleManagementControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<RoleManagementController>> _mockLogger;
        private readonly RoleManagementController _controller;

        public RoleManagementControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<RoleManagementController>>();
            _controller = new RoleManagementController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new RoleManagementController(_mockMediator.Object,null ));
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMediatorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new RoleManagementController( null, _mockLogger.Object));
            Assert.Equal("mediator", exception.ParamName);
        }


        [Fact]
        public async Task GetUserRoleById_ReturnsBadRequest_WhenUserRoleIdIsEmpty()
        {
            var result = await _controller.GetUserRoleById(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del rol de usuario no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetUserRoleById_ReturnsNotFound_WhenUserRoleDoesNotExist()
        {
            Guid userRoleId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByIdByUserIDQuery>(), default))
                         .ReturnsAsync((GetUserRoleDto)null);

            var result = await _controller.GetUserRoleById(userRoleId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró el rol de usuario con ID: {userRoleId}", result.Value);
        }

        [Fact]
        public async Task GetUserRoleById_ReturnsOk_WhenUserRoleExists()
        {
            Guid userRoleId = Guid.NewGuid();
            var mockUserRole = new GetUserRoleDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByIdByUserIDQuery>(), default))
                         .ReturnsAsync(mockUserRole);

            var result = await _controller.GetUserRoleById(userRoleId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockUserRole, result.Value);
        }

        [Fact]
        public async Task GetUserRoleById_ReturnsInternalServerError_WhenUnhandledExceptionOccurs()
        {
            Guid userRoleId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByIdByUserIDQuery>(), default))
                         .ThrowsAsync(new Exception("Error inesperado"));

            var result = await _controller.GetUserRoleById(userRoleId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el rol de usuario.", result.Value);
        }

        [Fact]
        public async Task GetUserRoleById_ReturnsInternalServerError_WhenDatabaseTimeoutOccurs()
        {
            Guid userRoleId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByIdByUserIDQuery>(), default))
                         .ThrowsAsync(new TimeoutException("Fallo de conexión con la base de datos"));

            var result = await _controller.GetUserRoleById(userRoleId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el rol de usuario.", result.Value);
        }

        [Fact]
        public async Task GetPermissionAll_ReturnsBadRequest_WhenArgumentNullExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPermissionAllQuery>(), default))
                         .ThrowsAsync(new ArgumentNullException());

            var result = await _controller.GetPermissionAll() as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Argumento inválido.", result.Value);
        }

        [Fact]
        public async Task GetPermissionAll_ReturnsInternalServerError_WhenTimeoutExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPermissionAllQuery>(), default))
                         .ThrowsAsync(new TimeoutException());

            var result = await _controller.GetPermissionAll() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener los permisos.", result.Value);
        }

        [Fact]
        public async Task GetPermissionAll_ReturnsInternalServerError_WhenInvalidOperationExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPermissionAllQuery>(), default))
                         .ThrowsAsync(new InvalidOperationException());

            var result = await _controller.GetPermissionAll() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Operación no válida.", result.Value);
        }

        [Fact]
        public async Task GetPermissionAll_ReturnsInternalServerError_WhenUnhandledExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPermissionAllQuery>(), default))
                         .ThrowsAsync(new Exception("Error inesperado"));

            var result = await _controller.GetPermissionAll() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener los permisos.", result.Value);
        }

        [Fact]
        public async Task GetRoleAll_ReturnsOk_WhenRolesExist()
        {
            var roles = new List<GetRoleDto> { new GetRoleDto() };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesAllQuery>(), default))
                .ReturnsAsync(roles);

            var result = await _controller.GetRoleAll() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(roles, result.Value);
        }

        [Fact]
        public async Task GetRoleAll_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesAllQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetRoleAll() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener los roles.", result.Value);
        }

        [Fact]
        public async Task GetRoleByName_ReturnsBadRequest_WhenRoleNameIsEmpty()
        {
            var result = await _controller.GetRoleByName("") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El nombre del rol no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetRoleByName_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            string roleName = "NotFoundRole";
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesByNameQuery>(), default))
                .ReturnsAsync((GetRoleDto)null);

            var result = await _controller.GetRoleByName(roleName) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un rol con nombre: {roleName}", result.Value);
        }

        [Fact]
        public async Task GetRoleByName_ReturnsOk_WhenRoleExists()
        {
            string roleName = "Admin";
            var mockRole = new GetRoleDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesByNameQuery>(), default))
                .ReturnsAsync(mockRole);

            var result = await _controller.GetRoleByName(roleName) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockRole, result.Value);
        }

        [Fact]
        public async Task GetRoleByName_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            string roleName = "Admin";

            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesByNameQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetRoleByName(roleName) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el rol.", result.Value);
        }

        [Fact]
        public async Task GetPermissionAll_ReturnsNotFound_WhenNoPermissionsExist()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPermissionAllQuery>(), default))
                .ReturnsAsync((List<GetPermissionDto>)null);

            var result = await _controller.GetPermissionAll() as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No se encontraron permisos en el sistema.", result.Value);
        }
        [Fact]
        public async Task GetUserRoleByIds_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            string userId = "user123";
            string roleId = "admin";

            _mockMediator.Setup(m => m.Send(It.IsAny<GetRoleByIdAndByUserIdQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetUserRoleByIds(userId, roleId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el rol de usuario.", result.Value);
        }

        [Fact]
        public async Task GetUserRoleByIds_ReturnsInternalServerError_WhenMediatorFails()
        {
            string userId = "user123";
            string roleId = "admin";

            _mockMediator.Setup(m => m.Send(It.IsAny<GetRoleByIdAndByUserIdQuery>(), default))
                .ThrowsAsync(new InvalidOperationException("Error inesperado en Mediator"));

            var result = await _controller.GetUserRoleByIds(userId, roleId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el rol de usuario.", result.Value);
        }
       

       /* [Fact]
        public async Task CreateRolePermission_ReturnsInternalServerError_WhenUnhandledExceptionOccurs()
        {
            var rolePermissionDto = new CreateRolePermissionDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateRolePermissionCommand>(), default))
                .ThrowsAsync(new Exception("Error inesperado"));

            var result = await _controller.CreateRolePermission(rolePermissionDto) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al asignar el permiso.", result.Value);
        }*/

        [Fact]
        public async Task DeleteRolePermission_ReturnsInternalServerError_WhenUnexpectedExceptionOccurs()
        {
            Guid rolePermissionId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteRolePermissionCommand>(), default))
                .ThrowsAsync(new Exception("Fallo crítico al eliminar el permiso"));

            var result = await _controller.DeleteRolePermission(rolePermissionId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al eliminar el permiso.", result.Value);
        }

        [Fact]
        public async Task GetUserRoleByIds_ReturnsInternalServerError_WhenDatabaseConnectionFails()
        {
            string userId = "user123";
            string roleId = "admin";

            _mockMediator.Setup(m => m.Send(It.IsAny<GetRoleByIdAndByUserIdQuery>(), default))
                .ThrowsAsync(new TimeoutException("Fallo de conexión con la base de datos"));

            var result = await _controller.GetUserRoleByIds(userId, roleId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el rol de usuario.", result.Value);
        }

        [Fact]
        public async Task GetPermissionAll_ReturnsOk_WhenPermissionsExist()
        {
            var permissions = new List<GetPermissionDto> { new GetPermissionDto() };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetPermissionAllQuery>(), default))
                .ReturnsAsync(permissions);

            var result = await _controller.GetPermissionAll() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(permissions, result.Value);
        }

        [Fact]
        public async Task GetPermissionAll_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPermissionAllQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetPermissionAll() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener los permisos.", result.Value);
        }


        [Fact]
        public async Task GetPermissionById_ReturnsBadRequest_WhenPermissionIdIsEmpty()
        {
            var result = await _controller.GetPermissionById(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del permiso no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetPermissionById_ReturnsNotFound_WhenPermissionDoesNotExist()
        {
            Guid permissionId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPemissionByIdQuery>(), default))
                .ReturnsAsync((GetPermissionDto)null);

            var result = await _controller.GetPermissionById(permissionId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un permiso con ID: {permissionId}", result.Value);
        }

        [Fact]
        public async Task GetPermissionById_ReturnsOk_WhenPermissionExists()
        {
            Guid permissionId = Guid.NewGuid();
            var mockPermission = new GetPermissionDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetPemissionByIdQuery>(), default))
                .ReturnsAsync(mockPermission);

            var result = await _controller.GetPermissionById(permissionId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockPermission, result.Value);
        }

        [Fact]
        public async Task GetPermissionById_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid permissionId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetPemissionByIdQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetPermissionById(permissionId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el permiso.", result.Value);
        }

        [Fact]
        public async Task GetRoleById_ReturnsBadRequest_WhenRoleIdIsEmpty()
        {
            var result = await _controller.GetRoleById(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del rol no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetRoleById_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            Guid roleId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesByIdQuery>(), default))
                         .ReturnsAsync((GetRoleDto)null);

            var result = await _controller.GetRoleById(roleId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un rol con ID: {roleId}", result.Value);
        }

        [Fact]
        public async Task GetRoleById_ReturnsOk_WhenRoleExists()
        {
            Guid roleId = Guid.NewGuid();
            var mockRole = new GetRoleDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesByIdQuery>(), default))
                         .ReturnsAsync(mockRole);

            var result = await _controller.GetRoleById(roleId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockRole, result.Value);
        }

        [Fact]
        public async Task GetRoleById_ReturnsNotFound_WhenKeyNotFoundExceptionOccurs()
        {
            Guid roleId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesByIdQuery>(), default))
                         .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.GetRoleById(roleId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un rol con ID: {roleId}", result.Value);
        }

        [Fact]
        public async Task CreateUserRole_ReturnsBadRequest_WhenDtoIsNull()
        {
            var result = await _controller.CreateUserRole(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Los datos de asignación de rol no pueden estar vacíos.", result.Value);
        }

        [Fact]
        public async Task CreateUserRole_ReturnsCreated_WhenRoleIsSuccessfullyAssigned()
        {
            var userRoleDto = new CreateUserRolesDto();
            Guid userRoleId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateUserRolesCommand>(), default))
                .ReturnsAsync(userRoleId);

            var result = await _controller.CreateUserRole(userRoleDto) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(nameof(_controller.GetUserRoleById), result.ActionName);
           
        }

       

       

        [Fact]
        public async Task GetRoleAll_ReturnsBadRequest_WhenArgumentNullExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesAllQuery>(), default))
                         .ThrowsAsync(new ArgumentNullException());

            var result = await _controller.GetRoleAll() as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Argumento inválido.", result.Value);
        }

        [Fact]
        public async Task GetRoleAll_ReturnsInternalServerError_WhenTimeoutExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesAllQuery>(), default))
                         .ThrowsAsync(new TimeoutException());

            var result = await _controller.GetRoleAll() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener los permisos.", result.Value);
        }

        [Fact]
        public async Task GetRoleAll_ReturnsInternalServerError_WhenInvalidOperationExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesAllQuery>(), default))
                         .ThrowsAsync(new InvalidOperationException());

            var result = await _controller.GetRoleAll() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Operación no válida.", result.Value);
        }

        [Fact]
        public async Task GetRoleAll_ReturnsInternalServerError_WhenUnhandledExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesAllQuery>(), default))
                         .ThrowsAsync(new Exception("Error inesperado"));

            var result = await _controller.GetRoleAll() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener los roles.", result.Value);
        }

        [Fact]
        public async Task CreateUserRole_ReturnsInternalServerError_WhenUnhandledExceptionOccurs()
        {
            var userRoleDto = new CreateUserRolesDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateUserRolesCommand>(), default))
                .ThrowsAsync(new Exception("Error inesperado"));

            var result = await _controller.CreateUserRole(userRoleDto) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al asignar rol.", result.Value);
        }

        [Fact]
        public async Task CreateUserRole_ReturnsInternalServerError_WhenDatabaseFails()
        {
            var userRoleDto = new CreateUserRolesDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateUserRolesCommand>(), default))
                .ThrowsAsync(new TimeoutException("Base de datos no responde"));

            var result = await _controller.CreateUserRole(userRoleDto) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al asignar rol.", result.Value);
        }

        [Fact]
        public async Task GetRoleById_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid roleId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesByIdQuery>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetRoleById(roleId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el rol.", result.Value);
        }

      /*  [Fact]
        public async Task CreateRolePermission_ReturnsBadRequest_WhenDtoIsNull()
        {
            var result = await _controller.CreateRolePermission(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Los datos de asignación de permiso no pueden ser nulos.", result.Value);
        }*/

        /*[Fact]
        public async Task CreateRolePermission_ReturnsCreated_WhenPermissionIsSuccessfullyAssigned()
        {
            var rolePermissionDto = new CreateRolePermissionDto();
            Guid rolePermissionId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateRolePermissionCommand>(), default))
                .ReturnsAsync(rolePermissionId);

            var result = await _controller.CreateRolePermission(rolePermissionDto) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(nameof(_controller.GetRoleById), result.ActionName);
          
        }*/

       /* [Fact]
        public async Task CreateRolePermission_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateRolePermissionCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.CreateRolePermission(new CreateRolePermissionDto()) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al asignar el permiso.", result.Value);
        }*/

        [Fact]
        public async Task GetAllRolePermission_ReturnsNotFound_WhenNoPermissionsExist()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesPermissionsAllQuery>(), default))
                .ReturnsAsync((List<GetRolePermissionDto>)null);

            var result = await _controller.GetAllRolePermission() as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No se encontraron permisos de roles en el sistema.", result.Value);
        }

        [Fact]
        public async Task GetAllRolePermission_ReturnsOk_WhenPermissionsExist()
        {
            var rolePermissions = new List<GetRolePermissionDto> { new GetRolePermissionDto() };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesPermissionsAllQuery>(), default))
                .ReturnsAsync(rolePermissions);

            var result = await _controller.GetAllRolePermission() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(rolePermissions, result.Value);
        }

        [Fact]
        public async Task GetAllRolePermission_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRolesPermissionsAllQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetAllRolePermission() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener permisos de roles.", result.Value);
        }


        [Fact]
        public async Task DeleteRolePermission_ReturnsBadRequest_WhenRolePermissionIdIsEmpty()
        {
            var result = await _controller.DeleteRolePermission(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del permiso de rol no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task DeleteRolePermission_ReturnsNotFound_WhenPermissionDoesNotExist()
        {
            Guid rolePermissionId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteRolePermissionCommand>(), default))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.DeleteRolePermission(rolePermissionId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró el permiso con ID: {rolePermissionId}", result.Value);
        }

       /* [Fact]
        public async Task DeleteRolePermission_ReturnsNoContent_WhenPermissionIsSuccessfullyDeleted()
        {
            Guid rolePermissionId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteRolePermissionCommand>(), default))
                .Returns(Task<RolePermissionId>);

            var result = await _controller.DeleteRolePermission(rolePermissionId) as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
        }*/

        [Fact]
        public async Task DeleteRolePermission_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid rolePermissionId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteRolePermissionCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.DeleteRolePermission(rolePermissionId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al eliminar el permiso.", result.Value);
        }

        [Fact]
        public async Task DeleteUserRole_ReturnsBadRequest_WhenRoleIdOrUserIdIsEmpty()
        {
            var result = await _controller.DeleteUserRole("", "") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID de rol y usuario no pueden estar vacíos.", result.Value);
        }

        [Fact]
        public async Task DeleteUserRole_ReturnsNotFound_WhenRoleDoesNotExistForUser()
        {
            string roleId = "admin";
            string userId = "user123";

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteUserRolesCommand>(), default))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.DeleteUserRole(roleId, userId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró el rol {roleId} para el usuario {userId}", result.Value);
        }

       /* [Fact]
        public async Task DeleteUserRole_ReturnsNoContent_WhenRoleIsSuccessfullyUnassigned()
        {
            string roleId = "admin";
            string userId = "user123";

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteUserRolesCommand>(), default))
                .Returns(Task.CompletedTask);

            var result = await _controller.DeleteUserRole(roleId, userId) as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
        }*/

        [Fact]
        public async Task DeleteUserRole_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            string roleId = "admin";
            string userId = "user123";

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteUserRolesCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.DeleteUserRole(roleId, userId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al eliminar rol.", result.Value);
        }

        [Fact]
        public async Task GetAllUserRole_ReturnsNotFound_WhenNoRolesExist()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUsersRolesQuery>(), default))
                .ReturnsAsync((List<GetUserRoleDto>)null);

            var result = await _controller.GetAllUserRole() as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No se encontraron roles de usuario en el sistema.", result.Value);
        }

        [Fact]
        public async Task GetAllUserRole_ReturnsOk_WhenRolesExist()
        {
            var userRoles = new List<GetUserRoleDto> { new GetUserRoleDto() };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUsersRolesQuery>(), default))
                .ReturnsAsync(userRoles);

            var result = await _controller.GetAllUserRole() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(userRoles, result.Value);
        }

        [Fact]
        public async Task GetAllUserRole_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUsersRolesQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetAllUserRole() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener roles de usuario.", result.Value);
        }

     

       
        [Fact]
        public async Task GetUserRoleById_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid userRoleId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByIdByUserIDQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetUserRoleById(userRoleId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el rol de usuario.", result.Value);
        }

        [Fact]
        public async Task GetUserRoleByName_ReturnsBadRequest_WhenUserRoleNameIsEmpty()
        {
            var result = await _controller.GetUserRoleByName("") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El nombre del rol de usuario no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetUserRoleByName_ReturnsNotFound_WhenUserRoleDoesNotExist()
        {
            string userRoleName = "NotFoundRole";
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByRoleNameQuery>(), default))
                .ReturnsAsync((List<GetUserRoleDto>)null); // Esto devuelve null correctamente

            var result = await _controller.GetUserRoleByName(userRoleName) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró el rol de usuario con nombre: {userRoleName}", result.Value);
        }

        [Fact]
        public async Task GetUserRoleByName_ReturnsOk_WhenUserRoleExists()
        {
            string userRoleName = "Admin";
            var mockUserRole = new List<GetUserRoleDto>();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByRoleNameQuery>(), default))
                .ReturnsAsync(mockUserRole);

            var result = await _controller.GetUserRoleByName(userRoleName) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockUserRole, result.Value);
        }

        [Fact]
        public async Task GetUserRoleByName_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            string userRoleName = "Admin";

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByRoleNameQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetUserRoleByName(userRoleName) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el rol de usuario.", result.Value);
        }

        [Fact]
        public async Task GetUserRoleByEmail_ReturnsBadRequest_WhenEmailIsEmpty()
        {
            var result = await _controller.GetUserRoleByEmail("") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El correo electrónico no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetUserRoleByEmail_ReturnsNotFound_WhenUserRoleDoesNotExist()
        {
            string userRoleEmail = "notfound@example.com";
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByUserEmailQuery>(), default))
                .ReturnsAsync((List<GetUserRoleDto>)null);

            var result = await _controller.GetUserRoleByEmail(userRoleEmail) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un rol de usuario con el correo electrónico: {userRoleEmail}", result.Value);
        }

        [Fact]
        public async Task GetUserRoleByEmail_ReturnsOk_WhenUserRoleExists()
        {
            string userRoleEmail = "test@example.com";
            var mockUserRole = new List<GetUserRoleDto>();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByUserEmailQuery>(), default))
                .ReturnsAsync(mockUserRole);

            var result = await _controller.GetUserRoleByEmail(userRoleEmail) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockUserRole, result.Value);
        }

        [Fact]
        public async Task GetUserRoleByEmail_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            string userRoleEmail = "test@example.com";

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserRolesByUserEmailQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetUserRoleByEmail(userRoleEmail) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el rol de usuario.", result.Value);
        }
    }
}
