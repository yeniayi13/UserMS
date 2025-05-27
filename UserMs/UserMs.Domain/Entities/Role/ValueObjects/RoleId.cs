using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.Role.ValueObjects
{
    public class RoleId : ValueObject
    {

        [BsonRepresentation(BsonType.Binary)]
        public Guid Value { get; }

        private RoleId(Guid value)
        {
            Value = value;
        }

        public RoleId()
        {

        }
        public static RoleId Create()
        {
            return new RoleId(Guid.NewGuid());
        }

        public static RoleId Create(Guid value)
        {
            return new RoleId(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {

            yield return Value;
        }
        public static implicit operator Guid(RoleId roleId) => roleId.Value;
        public static implicit operator RoleId(Guid value) => new RoleId(value);

    }
}
