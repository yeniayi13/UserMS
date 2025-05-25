using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Infrastructure.Exceptions;


[ExcludeFromCodeCoverage]
public class UserPasswordJsonConverter : JsonConverter<UserPassword>
{
    public override UserPassword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        return UserPassword.Create(value);
    }

    public override void Write(Utf8JsonWriter writer, UserPassword value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}