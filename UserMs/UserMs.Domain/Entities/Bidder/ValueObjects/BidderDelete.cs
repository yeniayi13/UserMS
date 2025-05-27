using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;

namespace UserMs.Domain.Entities.Bidder.ValueObjects
{
    public class BidderDelete : ValueObject
    {
        public bool Value { get; }

        private BidderDelete(bool value = false)
        {
            Value = value;
        }

        public static BidderDelete Create(bool value)
        {
            return new BidderDelete(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
