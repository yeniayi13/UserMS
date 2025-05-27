using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.Auctioneer.ValueObjects
{
    public class AuctioneerDelete : ValueObject
    {
        public bool Value { get; }

        private AuctioneerDelete(bool value = false)
        {
            Value = value;
        }

        public static AuctioneerDelete Create(bool value)
        {
            return new AuctioneerDelete(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
