using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.Support.ValueObjects
{
    public class SupportDelete : ValueObject
    {
        public bool Value { get; }

        private SupportDelete(bool value = false)
        {
            Value = value;
        }

        public static SupportDelete Create(bool value)
        {
            return new SupportDelete(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
