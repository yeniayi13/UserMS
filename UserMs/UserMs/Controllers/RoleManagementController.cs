using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserMs.Application.Commands.RolesPermission;
using UserMs.Application.Commands.UsersRoles;
using UserMs.Application.Queries.Permission;
using UserMs.Application.Queries.Roles;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.User_Roles.ValueObjects;
using UserMs.Commoon.Dtos.Users.Request.RolePermission;
using UserMs.Commoon.Dtos.Users.Request.UserRole;
using UserMs.Application.Queries.Roles_Permission;
using UserMs.Application.Queries.User_Roles;
using UserMs.Commoon.Dtos.Users.Response.Permission;

namespace UserMs.Controllers
{
    [ApiController]
    [Route("user/[controller]")]
    public class RoleManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RoleManagementController> _logger;

        public RoleManagementController(IMediator mediator, ILogger<RoleManagementController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        
        [HttpGet("Roles-All")]
        public async Task<IActionResult> GetRoleAll()
        {
            try
            {
                var query = new GetRolesAllQuery();
                var roles = await _mediator.Send(query);

                if (roles == null || !roles.Any())
                {
                    _logger.LogWarning("No se encontraron roles.");
                    return NotFound("No se encontraron roles en el sistema.");
                }

                return Ok(roles);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Error: argumento nulo.");
                return BadRequest("Argumento inválido.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Fallo de conexión con la base de datos.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al obtener los roles.{ex.Message}");
            }

            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operación inválida.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Operación no válida.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener roles.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al obtener los roles.{ex.Message}");
            }
        }

        [HttpGet("Roles/{roleId}")]
        public async Task<IActionResult> GetRoleById(Guid roleId)
        {
            if (roleId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud con ID de rol vacío.");
                return BadRequest("El ID del rol no puede estar vacío.");
            }

            try
            {
                var query = new GetRolesByIdQuery(roleId);
                var role = await _mediator.Send(query);

                if (role == null)
                {
                    _logger.LogWarning($"No se encontró un rol con ID: {roleId}");
                    return NotFound($"No se encontró un rol con ID: {roleId}");
                }

                return Ok(role);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"No se encontró el rol con ID: {roleId}");
                return NotFound($"No se encontró un rol con ID: {roleId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener rol con ID: {roleId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al buscar el rol. {ex.Message}");
            }
        }

        [HttpGet("Roles/Name/{roleName}")]
        public async Task<IActionResult> GetRoleByName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                _logger.LogWarning("Solicitud con nombre de rol vacío.");
                return BadRequest("El nombre del rol no puede estar vacío.");
            }

            try
            {
                var query = new GetRolesByNameQuery(roleName);
                var role = await _mediator.Send(query);

                if (role == null)
                {
                    _logger.LogWarning($"No se encontró un rol con nombre: {roleName}");
                    return NotFound($"No se encontró un rol con nombre: {roleName}");
                }

                return Ok(role);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"No se encontró el rol con nombre: {roleName}");
                return NotFound($"No se encontró un rol con nombre: {roleName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener rol con nombre: {roleName}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al buscar el rol. {ex.Message}");
            }
        }

        [Authorize(Policy = "AdministradorPolicy")]
        [HttpGet("Permission-All")]
        public async Task<IActionResult> GetPermissionAll()
        {
            try
            {
                var query = new GetPermissionAllQuery();
                var permissions = await _mediator.Send(query);

                if (permissions == null || !permissions.Any())
                {
                    _logger.LogWarning("No se encontraron permisos.");
                    return NotFound("No se encontraron permisos en el sistema.");
                }

                return Ok(permissions);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Error: argumento nulo.");
                return BadRequest("Argumento inválido.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Fallo de conexión con la base de datos.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al obtener los permisos. {ex.Message}");
            }

            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operación inválida.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Operación no válida.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener permisos.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al obtener los permisos.{ex.Message}");
            }
        }
       
       /* [Authorize(Policy = "AdministradorPolicy")]
        // 🔹 Asignar un permiso a un rol
        [HttpPost("Assign-Permission-Role")]
            public async Task<IActionResult> CreateRolePermission(
                [FromBody] CreateRolePermissionDto createRolePermissionDto)
            {
                if (createRolePermissionDto == null)
                {
                    _logger.LogWarning("Solicitud de asignación de permiso con datos nulos.");
                    return BadRequest("Los datos de asignación de permiso no pueden ser nulos.");
                }

                try
                {
                    var command = new CreateRolePermissionCommand(createRolePermissionDto);
                    var rolePermissionId = await _mediator.Send(command);
                    return CreatedAtAction(nameof(GetRoleById), new { rolePermissionId }, rolePermissionId);
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al asignar permisos.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al asignar el permiso.");
                }
            }*/


    
        [Authorize(Policy = "AdministradorPolicy")]
        [HttpGet("Roles-Permissions-All")]
        public async Task<IActionResult> GetAllRolePermission()
        {
            try
            {
                var query = new GetRolesPermissionsAllQuery();
                var rolePermissions = await _mediator.Send(query);

                if (rolePermissions == null || !rolePermissions.Any())
                {
                    _logger.LogWarning("No se encontraron permisos asignados.");
                    return NotFound("No se encontraron permisos de roles en el sistema.");
                }

                return Ok(rolePermissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener permisos de roles.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al obtener permisos de roles. {ex.Message}");
            }
        }


        [Authorize(Policy = "AdministradorPolicy")]
        [HttpDelete("Unassign-Permission-To-Role/{rolePermissionId}")]
        public async Task<IActionResult> DeleteRolePermission(Guid rolePermissionId)
        {
            if (rolePermissionId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud de eliminación de permiso con ID vacío.");
                return BadRequest("El ID del permiso de rol no puede estar vacío.");
            }

            try
            {
                var command = new DeleteRolePermissionCommand(rolePermissionId);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"No se encontró el permiso con ID: {rolePermissionId}");
                return NotFound($"No se encontró el permiso con ID: {rolePermissionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al eliminar permiso con ID: {rolePermissionId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al eliminar el permiso.{ex.Message}");
            }
        }

        [Authorize(Policy = "AdministradorPolicy")]

        // 🔹 Crear un rol de usuario
        [HttpPost("Assign-Roles-Users")]
        public async Task<IActionResult> CreateUserRole([FromBody] CreateUserRolesDto createUserRolesDto)
        {
            if (createUserRolesDto == null)
            {
                _logger.LogWarning("Solicitud de creación de rol de usuario con datos nulos.");
                return BadRequest("Los datos de asignación de rol no pueden estar vacíos.");
            }

            try
            {
                var command = new CreateUserRolesCommand(createUserRolesDto);
                var userRoleId = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetUserRoleById), new { userRoleId }, userRoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al asignar rol al usuario.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al asignar rol. {ex.Message}");
            }
        }
        [Authorize(Policy = "AdministradorPolicy")]
        // 🔹 Eliminar un rol de usuario
        [HttpDelete("Unassign-Roles-Users/{roleName}/{userEmail}")]
        public async Task<IActionResult> DeleteUserRole(string roleName, string userEmail)
        {
            if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(userEmail))
            {
                _logger.LogWarning("Solicitud de eliminación de rol con parámetros vacíos.");
                return BadRequest("El ID de rol y usuario no pueden estar vacíos.");
            }

            try
            {
                var command = new DeleteUserRolesCommand(roleName, userEmail);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"No se encontró el rol {roleName} para el usuario {userEmail}");
                return NotFound($"No se encontró el rol {roleName} para el usuario {userEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar rol de usuario.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al eliminar rol.{ex.Message}");
            }
        }

       
        [HttpGet("User-Roles-All")]
        public async Task<IActionResult> GetAllUserRole()
        {
            try
            {
                var query = new GetUsersRolesQuery();
                var userRoles = await _mediator.Send(query);

                if (userRoles == null || !userRoles.Any())
                {
                    _logger.LogWarning("No se encontraron roles de usuario.");
                    return NotFound("No se encontraron roles de usuario en el sistema.");
                }

                return Ok(userRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener roles de usuario.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al obtener roles de usuario. {ex.Message}");
            }
        }

        // 🔹 Obtener un rol de usuario por ID
        [HttpGet("User-Roles/{userRoleId}")]
        public async Task<IActionResult> GetUserRoleById(Guid userRoleId)
        {
            if (userRoleId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud de rol de usuario con ID vacío.");
                return BadRequest("El ID del rol de usuario no puede estar vacío.");
            }

            try
            {
                var query = new GetUserRolesByIdByUserIDQuery(userRoleId);
                var userRole = await _mediator.Send(query);

                if (userRole == null)
                {
                    _logger.LogWarning($"No se encontró el rol de usuario con ID: {userRoleId}");
                    return NotFound($"No se encontró el rol de usuario con ID: {userRoleId}");
                }

                return Ok(userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener rol de usuario con ID: {userRoleId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al buscar el rol de usuario. {ex.Message}");
            }
        }


        // 🔹 Obtener un rol de usuario por nombre
        [HttpGet("User-Roles/Name/{userRoleName}")]
        public async Task<IActionResult> GetUserRoleByName(string userRoleName)
        {
            if (string.IsNullOrWhiteSpace(userRoleName))
            {
                _logger.LogWarning("Solicitud de rol de usuario con nombre vacío.");
                return BadRequest("El nombre del rol de usuario no puede estar vacío.");
            }

            try
            {
                var query = new GetUserRolesByRoleNameQuery(userRoleName);
                var userRole = await _mediator.Send(query);

                if (userRole == null)
                {
                    _logger.LogWarning($"No se encontró el rol de usuario con nombre: {userRoleName}");
                    return NotFound($"No se encontró el rol de usuario con nombre: {userRoleName}");
                }

                return Ok(userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener rol de usuario por nombre: {userRoleName}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al buscar el rol de usuario.");
            }
        }


        // 🔹 Obtener un rol de usuario por email
        [HttpGet("User-Roles/Email/{userRoleEmail}")]
        public async Task<IActionResult> GetUserRoleByEmail([FromRoute] string userRoleEmail)
        {
            if (string.IsNullOrWhiteSpace(userRoleEmail))
            {
                _logger.LogWarning("Solicitud con correo electrónico vacío.");
                return BadRequest("El correo electrónico no puede estar vacío.");
            }

            try
            {
                var query = new GetUserRolesByUserEmailQuery(userRoleEmail);
                var userRole = await _mediator.Send(query);

                if (userRole == null)
                {
                    _logger.LogWarning($"No se encontró un rol de usuario con el correo electrónico: {userRoleEmail}");
                    return NotFound($"No se encontró un rol de usuario con el correo electrónico: {userRoleEmail}");
                }

                return Ok(userRole);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"No se encontró un rol de usuario con el correo electrónico: {userRoleEmail}");
                return NotFound($"No se encontró un rol de usuario con el correo electrónico: {userRoleEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener el rol de usuario por correo electrónico.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al buscar el rol de usuario.");
            }
        }

        // 🔹 Obtener un rol de usuario por ID de usuario y ID de rol
        [HttpGet("User-Roles/Id/{userId}/role/Id/{roleId}")]
        public async Task<IActionResult> GetUserRoleByIds([FromRoute] string userId, [FromRoute] string roleId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleId))
            {
                _logger.LogWarning("Solicitud con ID de usuario o ID de rol vacío.");
                return BadRequest("El ID del usuario y el ID del rol no pueden estar vacíos.");
            }

            try
            {
                var query = new GetRoleByIdAndByUserIdQuery(roleId, userId);
                var userRole = await _mediator.Send(query);

                if (userRole == null)
                {
                    _logger.LogWarning($"No se encontró un rol de usuario con el ID: {roleId} para el usuario: {userId}");
                    return NotFound($"No se encontró un rol de usuario con el ID: {roleId} para el usuario: {userId}");
                }

                return Ok(userRole);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"No se encontró un rol de usuario con el ID: {roleId} para el usuario: {userId}");
                return NotFound($"No se encontró un rol de usuario con el ID: {roleId} para el usuario: {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener el rol de usuario por ID.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al buscar el rol de usuario.");
            }
        }


    }
}
