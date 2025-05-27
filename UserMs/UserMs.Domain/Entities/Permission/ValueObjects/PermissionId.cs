using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Domain.Entities.Permission.ValueObjects
{
    public class PermissionId : ValueObject
    {

        [BsonRepresentation(BsonType.Binary)]
        public Guid Value { get; }

        private PermissionId(Guid value)
        {
            Value = value;
        }

        public PermissionId()
        {

        }
        public static PermissionId Create()
        {
            return new PermissionId(Guid.NewGuid());
        }

        public static PermissionId Create(Guid value)
        {
            return new PermissionId(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {

            yield return Value;
        }
        public static implicit operator Guid(PermissionId permissionId) => permissionId.Value;
        public static implicit operator PermissionId(Guid value) => new PermissionId(value);

    }
}
