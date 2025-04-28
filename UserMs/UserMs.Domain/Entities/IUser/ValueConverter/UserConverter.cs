using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.IUser.ValueObjects;

public class UserEmailValueConverter : ValueConverter<UserEmail, string>
{
    public UserEmailValueConverter() : base(
        v => v.Value, // Convierte UserEmail a string
        v => UserEmail.Create(v) // Convierte string a UserEmail
    ) { }
}

public class UserIdValueConverter : ValueConverter<UserId, Guid>
{
    public UserIdValueConverter() : base(
        v => v.Value, // Convierte UserId a Guid
        v => UserId.Create(v) // Convierte Guid a UserId
    ) { }
}

public class UserPasswordValueConverter : ValueConverter<UserPassword, string>
{
    public UserPasswordValueConverter() : base(
        v => v.Value, // Convierte UserPassword a string
        v => UserPassword.Create(v) // Convierte string a UserPassword
    ) { }
}

public class UserAddressValueConverter : ValueConverter<UserAddress, string>
{
    public UserAddressValueConverter() : base(
        v => v.Value, // Convierte UserPassword a string
        v => UserAddress.Create(v) // Convierte string a UserPassword
    )
    { }
}

public class UserNameValueConverter : ValueConverter<UserName, string>
{
    public UserNameValueConverter() : base(
        v => v.Value, // Convierte UserPassword a string
        v => UserName.Create(v) // Convierte string a UserPassword
    )
    { }
}


public class UserPhoneValueConverter : ValueConverter<UserPhone, string>
{
    public UserPhoneValueConverter() : base(
        v => v.Value, // Convierte UserPassword a string
        v => UserPhone.Create(v) // Convierte string a UserPassword
    )
    { }
}

public class UserDeleteConverter : ValueConverter<UserDelete, bool>
{
    public UserDeleteConverter() : base(
        u => u.Value,
        b => UserDelete.Create(b)
    ) { }
}

