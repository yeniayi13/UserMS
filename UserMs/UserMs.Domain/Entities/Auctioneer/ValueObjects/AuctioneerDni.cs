using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.Auctioneer.ValueObjects
{
    public class AuctioneerDni : ValueObject
    {
        private AuctioneerDni(string value) => Value = value;

        public static AuctioneerDni Create(string value)
        {

            //if (string.IsNullOrEmpty(value)) throw new NullAtributeException("UserS address is required");

            return new AuctioneerDni(value);
        }

        public string Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
