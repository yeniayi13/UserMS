using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.User_Roles.Queries;
using UserMs.Application.Queries.User_Roles;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Domain.User_Roles;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.User_Role
{
    public class GetUsersRolesQueryHandlerTests
    {
        private readonly Mock<IUserRoleRepositoryMongo> _userRoleRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetUsersRolesQueryHandler _handler;

        public GetUsersRolesQueryHandlerTests()
        {
            _userRoleRepositoryMock = new Mock<IUserRoleRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetUsersRolesQueryHandler(
                _userRoleRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando existen roles de usuarios, se devuelven correctamente los datos mapeados.
        /// </summary>
       /* [Fact]
        public async Task Handle_ShouldReturnMappedUserRoles_WhenUserRolesExist()
        {
            // Arrange
            var userRoles = new List<UserRoles>
        {
            new UserRoles { UserRoleId = Guid.NewGuid(), UserId = Guid.NewGuid() },
            new UserRoles { UserRoleId = Guid.NewGuid(), UserId = Guid.NewGuid()}
        };
            var expectedDtos = new List<GetUserRoleDto>
        {
            new GetUserRoleDto { UserRoleId = userRoles[0].UserRoleId, UserId = userRoles[0].UserId },
            new GetUserRoleDto { UserRoleId = userRoles[1].UserRoleId, UserId = userRoles[1].UserId }
        };

            _userRoleRepositoryMock
                .Setup(repo => repo.GetUsersRoleAsync())
                .ReturnsAsync(expectedDtos);

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetUserRoleDto>>(userRoles))
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(new GetUsersRolesQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDtos.Count, result.Count);
            Assert.Equal(expectedDtos[0].RoleName, result[0].RoleName);
            Assert.Equal(expectedDtos[1].RoleName, result[1].RoleName);
        }*/

        /// <summary>
        /// Verifica que cuando no hay roles de usuarios, se retorne una lista vacía.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoUserRolesFound()
        {
            // Arrange
            _userRoleRepositoryMock
                .Setup(repo => repo.GetUsersRoleAsync())
                .ReturnsAsync(new List<GetUserRoleDto>()); // Lista vacía

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetUserRoleDto>>(It.IsAny<List<UserRoles>>()))
                .Returns(new List<GetUserRoleDto>()); // Simulación de mapeo

            // Act
            var result = await _handler.Handle(new GetUsersRolesQuery(), CancellationToken.None);

            // Assert
            Assert.NotNull(result); // Validamos que la respuesta no sea `null`
            Assert.Empty(result); // Verificamos que la lista esté vacía
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            _userRoleRepositoryMock
                .Setup(repo => repo.GetUsersRoleAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetUsersRolesQuery(), CancellationToken.None));
        }
    }

}
