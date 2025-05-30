using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UserMs.Domain.Entities.IUser.ValueObjects;

namespace UserMs.Domain.Entities
{
    public class Base
    {
        //   [BsonRepresentation(BsonType.String)]
        //[BsonElement("UserId")]
        public UserId UserId { get;  set; } // Make it private to enforce immutability
        public UserEmail UserEmail { get;  set; }
        public UserPassword UserPassword { get;  set; }
        public UserAddress? UserAddress { get;  set; }
        public UserPhone? UserPhone { get;  set; }
        public UserName UserName { get;  set; }
        public UserLastName UserLastName { get;  set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }


        protected Base(UserId userId, UserEmail userEmail, UserPassword userPassword, UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName)
        {
            UserId = userId;
            UserEmail = userEmail;
            UserPassword = userPassword;
            UserName = userName;
            UserPhone = userPhone;
            UserAddress = userAddress;
            UserLastName = userLastName;
           

        }
        protected Base(UserEmail userEmail, UserPassword userPassword, UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName)
        {
            UserEmail = userEmail;
            UserPassword = userPassword;
            UserName = userName;
            UserPhone = userPhone;
            UserAddress = userAddress;
            UserLastName = userLastName;

        }

        protected Base(UserId userId)
        { 
            UserId = userId;
        }

        protected Base(UserId userId, UserName userName)
        {
            UserId = userId;
            UserName = userName;
        }
        protected Base()
        {
            
        }

        public void SetUserId(UserId userId)
        {
            UserId = userId;
        }

        public void SetUserEmail(UserEmail userEmail)
        {
            UserEmail = userEmail;
        }

        public void SetUserPassword(UserPassword userPassword)
        {
            UserPassword = userPassword;
        }



        public void SetUserAddress(UserAddress userAddress)
        {
            UserAddress = userAddress;
        }

        public void SetUserPhone(UserPhone userPhone)
        {
            UserPhone = userPhone;
        }

        public void SetUserName(UserName userName)
        {
            UserName = userName;
        }

        public void SetUserLastName(UserLastName userLastName)
        {
            UserLastName = userLastName;
        }

    }
}