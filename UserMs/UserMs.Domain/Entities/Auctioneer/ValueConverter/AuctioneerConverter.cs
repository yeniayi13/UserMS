using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;

public class AuctioneerDniValueConverter : ValueConverter<AuctioneerDni, string>
{
    public AuctioneerDniValueConverter() : base(
        v => v.Value, // Convierte DNI a string
        v => AuctioneerDni.Create(v) // Convierte string a BidderDni
    )
    { }
}

public class AuctioneerIdValueConverter : ValueConverter<AuctioneerId, Guid>
{
    public AuctioneerIdValueConverter() : base(
        v => v.Value, // Convierte UserId a Guid
        v => AuctioneerId.Create(v) // Convierte Guid a UserId
    )
    { }
}

public class AuctioneerUserIdValueConverter : ValueConverter<AuctioneerUserId, Guid>
{
    public AuctioneerUserIdValueConverter() : base(
        v => v.Value, // Convierte UserPassword a string
        v => AuctioneerUserId.Create(v) // Convierte string a UserPassword
    )
    { }

}