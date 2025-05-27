using MediatR;
using UserMs.Commoon.Dtos.Users.Response.User;

namespace UserMs.Application.Queries.User
{
    public class GetUsersQuery : IRequest<List<GetUsersDto>>
    {
        public Guid UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPassword { get; set; }
        public string? UserType { get; set; }
       
    }
}