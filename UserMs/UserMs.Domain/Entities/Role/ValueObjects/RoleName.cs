using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Permission.ValueObjects;

namespace UserMs.Domain.Entities.Role.ValueObjects
{
    public class RoleName : ValueObject
    {
        public string Value { get; }

        private RoleName(string value)
        {
            Value = value;
        }

        public RoleName()
        {

        }
        public static RoleName Create(string value)
        {
            return new RoleName(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
