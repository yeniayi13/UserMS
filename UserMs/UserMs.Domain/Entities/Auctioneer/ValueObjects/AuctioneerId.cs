using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Bidder.ValueObjects;

namespace UserMs.Domain.Entities.Auctioneer.ValueObjects
{
    public class AuctioneerId : ValueObject
    {
        private AuctioneerId(Guid value) => Value = value;

        public static AuctioneerId Create(Guid value)
        {

            return new AuctioneerId(value);
        }

        public static AuctioneerId Create()
        {
            return new AuctioneerId(Guid.NewGuid());
        }

        public Guid Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
