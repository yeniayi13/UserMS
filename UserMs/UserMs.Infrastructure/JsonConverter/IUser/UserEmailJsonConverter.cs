using System.Text.Json;
using System.Text.Json.Serialization;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Exceptions;

public class UserEmailJsonConverter : JsonConverter<UserEmail>
{
    public override UserEmail Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        return UserEmail.Create(value);
    }

    public override void Write(Utf8JsonWriter writer, UserEmail value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}