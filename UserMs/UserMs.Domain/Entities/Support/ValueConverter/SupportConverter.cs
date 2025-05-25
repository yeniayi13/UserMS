using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Support.ValueObjects;

public class SupportDniValueConverter : ValueConverter<SupportDni, string>
{
    public SupportDniValueConverter() : base(
        v => v.Value, // Convierte DNI a string
        v => SupportDni.Create(v) // Convierte string a BidderDni
    )
    { }
}

public class SupportIdValueConverter : ValueConverter<SupportId, Guid>
{
    public SupportIdValueConverter() : base(
        v => v.Value, // Convierte UserId a Guid
        v => SupportId.Create(v) // Convierte Guid a UserId
    )
    { }
}

public class SupportUserIdValueConverter : ValueConverter<SupportUserId, Guid>
{
    public SupportUserIdValueConverter() : base(
        v => v.Value, // Convierte UserPassword a string
        v => SupportUserId.Create(v) // Convierte string a UserPassword
    )
    { }

}




public class SupportDeleteConverter : ValueConverter<SupportDelete, bool>
{
    public SupportDeleteConverter() : base(
        u => u.Value,
        b => SupportDelete.Create(b)
    )
    { }
}