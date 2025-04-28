using UserMs.Application.Commands.User;
using UserMs.Application.Validators;
using UserMs.Core.Repositories;
using UserMs.Domain.Entities;
using MediatR;
using UserMs.Core.Interface;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Infrastructure.Exceptions;


namespace UserMs.Application.Handlers.User.Commands
{
    public class CreateUsersCommandHandler : IRequestHandler<CreateUsersCommand, UserId>
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IAuthMsService _authMsService;

        public CreateUsersCommandHandler(IUsersRepository usersRepository, IAuthMsService authMsService)
        {
            _usersRepository = usersRepository;
            _authMsService = authMsService;
        }



        public async Task<UserId> Handle(CreateUsersCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var validator = new CreateUsersValidator();
                await validator.ValidateRequest(request.Users);
                var userId = request.Users.UserId;
                var usersEmailValue = request.Users.UserEmail;
                var usersPasswordValue = request.Users.UserPassword;
                var usersNameValue = request.Users.UserName;
                var usersPhoneValue = request.Users.UserPhone;
                var usersAddressValue = request.Users.UserAddress;
                //var usersDepartamentValue = request.Users.ProviderDepartmentId;

               // await _authMsService.CreateUserAsync(usersEmailValue!, usersPasswordValue);
                //var userId = await _authMsService.GetUserByUserName(UserEmail.Create(usersEmailValue!));
               // await _authMsService.AssignClientRoleToUser(userId, request.Users.UsersType.ToString()!);

                var users = new Users(
                    UserId.Create(userId),
                    UserEmail.Create(usersEmailValue ),
                    UserPassword.Create(usersPasswordValue ),
                    UserName.Create(usersNameValue ),
                    UserPhone.Create(usersPhoneValue ),
                    UserAddress.Create(usersAddressValue),
                    Enum.Parse<UsersType>(request.Users.UsersType.ToString()!),
                    Enum.Parse<UserAvailable>(request.Users.UserAvailable.ToString()!)


                );

                if (request.Users.UsersType == null)
                {
                    throw new NullAtributeException("UsersType can't be null");
                }


                await _usersRepository.AddAsync(users);

                return users.UserId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}