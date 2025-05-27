using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.IUser.ValueObjects;

namespace UserMs.Domain.Entities.Permission.ValueObjects
{
    public class PermissionName : ValueObject
    {
        public string Value { get; }

        private PermissionName(string value)
        {
            Value = value;
        }

        public PermissionName()
        {

        }
        public static PermissionName Create(string value)
        {
            return new PermissionName(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
