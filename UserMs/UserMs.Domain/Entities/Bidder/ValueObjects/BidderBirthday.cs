using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.Bidder.ValueObjects
{
    public class BidderBirthday : ValueObject
    {
        private BidderBirthday(DateOnly value) => Value = value;

        public static BidderBirthday Create(DateOnly value)
        {

            //if (string.IsNullOrEmpty(value)) throw new NullAtributeException("UserS address is required");

            return new BidderBirthday(value);
        }

        public DateOnly Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
