using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.Bidder.ValueObjects
{
    public class AuctioneerBirthday : ValueObject
    {
        private AuctioneerBirthday(DateOnly value) => Value = value;

        public static AuctioneerBirthday Create(DateOnly value)
        {

            //if (string.IsNullOrEmpty(value)) throw new NullAtributeException("UserS address is required");

            return new AuctioneerBirthday(value);
        }

        public DateOnly Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
