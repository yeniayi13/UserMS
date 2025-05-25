using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role.ValueObjects;

namespace UserMs.Domain.User_Roles.ValueObjects
{
    public class UserRoleId : ValueObject
    {

       // [BsonRepresentation(BsonType.Binary)]
       // [BsonRepresentation(BsonType.Binary)]
        public Guid Value { get; }

        private UserRoleId(Guid value)
        {
            Value = value;
        }

        public UserRoleId()
        {

        }
        public static UserRoleId Create()
        {
            return new UserRoleId(Guid.NewGuid());
        }

        public static UserRoleId Create(Guid value)
        {
            return new UserRoleId(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {

            yield return Value;
        }
        public static implicit operator Guid(UserRoleId roleId) => roleId.Value;
        public static implicit operator UserRoleId(Guid value) => new UserRoleId(value);

    }
}
