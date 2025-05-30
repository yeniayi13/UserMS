using System.Text.Json.Serialization;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.User_Roles;

namespace UserMs.Domain.Entities.UserEntity

{
    public class Users : Base
    {
        public UsersType UsersType { get; private set; }
        public UserAvailable UserAvailable { get; private set; }
        public ICollection<UserRoles> UserRoles { get; set; }
        public UserDelete? UserDelete { get;  set; }
        public ICollection<ActivityHistory.ActivityHistory> ActivityHistories { get; set; }


        // public ICollection<Supports> Supports { get; private set; }
        // public ICollection<Auctioneers> Auctioneers { get; private set; }
        // public ICollection<Bidders> Bidders { get; private set; }


        [JsonConstructor]
        public Users(UserId userId, UserEmail userEmail, UserPassword userPassword,UserName userName,UserPhone userPhone,UserAddress userAddress, UserLastName userLastName, UsersType usersType, UserAvailable userAvailable)
            : base(userId, userEmail, userPassword,userName,userPhone,userAddress,userLastName)
        {
            UsersType = usersType;
            UserAvailable = userAvailable;
        }

        public Users(UserId userId, UserEmail userEmail, UserPassword userPassword, UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName, UsersType usersType, UserAvailable userAvailable, UserDelete userDelete)
            : base(userId, userEmail, userPassword, userName, userPhone, userAddress, userLastName)
        {
            UsersType = usersType;
            UserAvailable = userAvailable;
            UserDelete = userDelete;
        }

        public Users( UserEmail userEmail, UserPassword userPassword, UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName, UsersType usersType, UserAvailable userAvailable)
            : base( userEmail, userPassword, userName, userPhone, userAddress, userLastName)
        {
            UsersType = usersType;
            UserAvailable = userAvailable;
        }

        public Users(UserId userId, UserEmail userEmail, UserPassword userPassword, UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName)
            : base(userId,userEmail, userPassword, userName, userPhone, userAddress, userLastName)
        {
           
           
        }

        public Users(UserId userId): base(userId)
        {
        }
        public Users(UserId userId, UserName userName) : base(userId,userName)
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

        public void SetUserDelete(UserDelete userDelete)
        {
            UserDelete = userDelete;
        }
    }
}