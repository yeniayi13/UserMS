using System.Text.Json.Serialization;
using UserMs.Domain.Entities.IUser.ValueObjects;

namespace UserMs.Domain.Entities
{
    public class Users : Base
    {
        public UsersType UsersType { get; private set; }
        public UserAvailable UserAvailable { get; private set; }

        [JsonConstructor]
        public Users(UserId userId, UserEmail userEmail, UserPassword userPassword,UserName userName,UserPhone userPhone,UserAddress userAddress, UserLastName userLastName, UsersType usersType, UserAvailable userAvailable)
            : base(userId, userEmail, userPassword,userName,userPhone,userAddress,userLastName)
        {
            UsersType = usersType;
            UserAvailable = userAvailable;
        }

        public Users(UserId userId, UserEmail userEmail, UserPassword userPassword, UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName, UsersType usersType, UserAvailable userAvailable, UserDelete userDelete)
            : base(userId, userEmail, userPassword, userName, userPhone, userAddress, userLastName,userDelete)
        {
            UsersType = usersType;
            UserAvailable = userAvailable;
        }

        public Users( UserEmail userEmail, UserPassword userPassword, UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName, UsersType usersType, UserAvailable userAvailable)
            : base( userEmail, userPassword, userName, userPhone, userAddress, userLastName)
        {
            UsersType = usersType;
            UserAvailable = userAvailable;
        }

        

        public Users(UserId userId): base(userId)
        {
        }

        public Users() 
        {
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