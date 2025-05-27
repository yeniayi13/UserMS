

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace UserMs.Domain.Entities
{
    public class UserId : ValueObject
        {

       // [BsonRepresentation(BsonType.Binary)]
        public Guid Value { get; }

            private UserId(Guid value)
            {
                Value = value;
            }

        public UserId()
            {
                
            }
        public static UserId Create()
        {
                return new UserId(Guid.NewGuid());
        }

        public static UserId Create(Guid value)
        {
                return new UserId(value);
        }

       protected override IEnumerable<object> GetEqualityComponents() { 
        
                yield return Value;
       }
        public static implicit operator Guid(UserId userId) => userId.Value;
        public static implicit operator UserId(Guid value) => new UserId(value);

    }
}