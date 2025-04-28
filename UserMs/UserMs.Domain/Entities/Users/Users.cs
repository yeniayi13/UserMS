using System.Text.Json.Serialization;
using UserMs.Domain.Entities.IUser.ValueObjects;

namespace UserMs.Domain.Entities
{
    public class Users : Base
    {
        public UsersType UsersType { get; private set; }
        public UserAvailable UserAvailable { get; private set; }

        [JsonConstructor]
        public Users(UserId userId, UserEmail userEmail, UserPassword userPassword,UserName userName,UserPhone userPhone,UserAddress userAddress,UsersType usersType, UserAvailable userAvailable)
            : base(userId, userEmail, userPassword,userName,userPhone,userAddress)
        {
            UsersType = usersType;
            UserAvailable = userAvailable;
        }

        public string GetUsersTypeString()
        {
            return UsersType.ToString();
        }

        public void SetUsersType(UsersType usersType)
        {
            UsersType = usersType;
        }

        public void SetUsersAvailable(UserAvailable userAvailable)
        {
            UserAvailable = userAvailable;
        }
    }
}