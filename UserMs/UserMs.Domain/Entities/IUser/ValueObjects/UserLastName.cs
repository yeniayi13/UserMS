using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.IUser.ValueObjects
{
   
        public class UserLastName : ValueObject
        {
            public string Value { get; }

            private UserLastName(string value)
            {
                Value = value;
            }

            public UserLastName()
            {

            }
            public static UserLastName Create(string value)
            {
                return new UserLastName(value);
            }

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Value;
            }
        }
    
}
