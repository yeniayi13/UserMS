using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.IUser.ValueObjects
{

    public class UserName : ValueObject
    {
        public string Value { get; }

        private UserName(string value)
        {
            Value = value;
        }

        public UserName()
        {

        }
        public static UserName Create(string value)
        {
            return new UserName(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
