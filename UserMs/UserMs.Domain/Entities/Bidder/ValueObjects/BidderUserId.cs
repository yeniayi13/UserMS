using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.IUser.ValueObjects;

namespace UserMs.Domain.Entities.Auctioneer.ValueObjects
{
    public class BidderUserId : ValueObject
    {
        private BidderUserId(Guid value) => Value = value;

        public static BidderUserId Create(Guid value)
        {

            return new BidderUserId(value);
        }

        public Guid Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
