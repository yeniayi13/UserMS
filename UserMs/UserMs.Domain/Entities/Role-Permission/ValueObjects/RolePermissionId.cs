using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role.ValueObjects;

namespace UserMs.Domain.Entities.Role_Permission.ValueObjects
{
    public class RolePermissionId : ValueObject
    {

        [BsonRepresentation(BsonType.Binary)]
        public Guid Value { get; }

        private RolePermissionId(Guid value)
        {
            Value = value;
        }

        public RolePermissionId()
        {

        }
        public static RolePermissionId Create()
        {
            return new RolePermissionId(Guid.NewGuid());
        }

        public static RolePermissionId Create(Guid value)
        {
            return new RolePermissionId(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {

            yield return Value;
        }
        public static implicit operator Guid(RolePermissionId rolePermisionId) => rolePermisionId.Value;
        public static implicit operator RolePermissionId(Guid value) => new RolePermissionId(value);

    }
}
