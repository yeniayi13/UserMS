using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Infrastructure.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace UserMs.Infrastructure.JsonConverter.IUser
{
    [ExcludeFromCodeCoverage]
    public class UserNameJsonConverter : JsonConverter<UserName>
    {
        public override UserName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return UserName.Create(value);
        }

        public override void Write(Utf8JsonWriter writer, UserName value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
