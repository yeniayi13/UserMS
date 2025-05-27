using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Bidder.ValueObjects;

namespace UserMs.Domain.Entities.Support.ValueObjects
{
    public class SupportUserId : ValueObject
    {
        private SupportUserId(Guid value) => Value = value;

        public static SupportUserId Create(Guid value)
        {


            return new SupportUserId(value);
        }

        public Guid Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
