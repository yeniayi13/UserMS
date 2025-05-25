using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.IUser.ValueObjects;

namespace UserMs.Infrastructure.JsonConverter.IUser
{
    [ExcludeFromCodeCoverage]
    public class UserLastNameJsonConverter : JsonConverter<UserLastName>
    {
        public override UserLastName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return UserLastName.Create(value);
        }

        public override void Write(Utf8JsonWriter writer, UserLastName value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
