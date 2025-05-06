
using MediatR;
using UserMs.Application.Dtos.Users.Response;
using UserMs.Domain.Entities;

namespace UserMs.Application.Queries.User
{
    public class GetUsersByIdQuery : IRequest<GetUsersDto>
    {
        public Guid Id { get; set; }

        public GetUsersByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}