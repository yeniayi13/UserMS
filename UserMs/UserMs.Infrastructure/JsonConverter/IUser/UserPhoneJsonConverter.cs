using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Infrastructure.Exceptions;
using UserMs.Domain.Entities;

namespace UserMs.Infrastructure.JsonConverter.IUser
{
    public class UserPhoneJsonConverter : JsonConverter<UserPhone>
    {
        public override UserPhone Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return UserPhone.Create(value);
        }

        public override void Write(Utf8JsonWriter writer, UserPhone value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
