using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;

public class BidderDniValueConverter : ValueConverter<BidderDni, string>
{
    public BidderDniValueConverter() : base(
        v => v.Value, // Convierte DNI a string
        v => BidderDni.Create(v) // Convierte string a BidderDni
    ) { }
}

public class BidderIdValueConverter : ValueConverter<BidderId, Guid>
{
    public BidderIdValueConverter() : base(
        v => v.Value, // Convierte UserId a Guid
        v => BidderId.Create(v) // Convierte Guid a UserId
    ) { }
}

public class BidderUserIdValueConverter : ValueConverter<BidderUserId, Guid>
{
    public BidderUserIdValueConverter() : base(
        v => v.Value, // Convierte UserPassword a string
        v => BidderUserId.Create(v) // Convierte string a UserPassword
    ) { }

}
public class BidderBirthdayValueConverter : ValueConverter<BidderBirthday, DateOnly>
{
    public BidderBirthdayValueConverter() : base(
        v => v.Value, // Convierte UserPassword a string
        v => BidderBirthday.Create(v) // Convierte string a UserPassword
    )
    { }


}


public class BidderDeleteConverter : ValueConverter<BidderDelete, bool>
{
    public BidderDeleteConverter() : base(
        u => u.Value,
        b => BidderDelete.Create(b)
    )
    { }
}

