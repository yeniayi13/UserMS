using UserMs.Domain.Entities.IUser.ValueObjects;

namespace UserMs.Domain.Entities
{
    public class Base
    {
        public UserId UserId { get; private set; } // Make it private to enforce immutability
        public UserEmail UserEmail { get; private set; }
        public UserPassword UserPassword { get; private set; }
        public UserDelete? UserDelete { get; private set; }
        public UserAddress? UserAddress { get; private set; }
        public UserPhone? UserPhone { get; private set; }
        public UserName UserName { get; private set; }
        public UserLastName UserLastName { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        protected Base(UserId userId, UserEmail userEmail, UserPassword userPassword,UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName)
        {
            UserId = userId;
            UserEmail = userEmail;
            UserPassword = userPassword;
            UserName = userName;
            UserPhone = userPhone;
            UserAddress = userAddress;
            UserLastName = userLastName;

        }

       

        public void SetUserEmail(UserEmail userEmail)
        {
            UserEmail = userEmail;
        }

        public void SetUserPassword(UserPassword userPassword)
        {
            UserPassword = userPassword;
        }

        public void SetUserDelete(UserDelete userDelete)
        {
            UserDelete = userDelete;
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