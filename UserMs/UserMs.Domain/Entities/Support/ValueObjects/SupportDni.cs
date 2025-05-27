using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.Support.ValueObjects
{
    public class SupportDni : ValueObject
    {
        private SupportDni(string value) => Value = value;

        public static SupportDni Create(string value)
        {

            //if (string.IsNullOrEmpty(value)) throw new NullAtributeException("UserS address is required");

            return new SupportDni(value);
        }

        public string Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
