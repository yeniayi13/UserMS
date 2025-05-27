using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.Support.ValueObjects
{
    public class SupportId : ValueObject
    {
        private SupportId(Guid value) => Value = value;

        public static SupportId Create(Guid value)
        {
            return new SupportId(value);
        }

        public static SupportId Create()
        {
            return new SupportId(Guid.NewGuid());
        }

        public Guid Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
