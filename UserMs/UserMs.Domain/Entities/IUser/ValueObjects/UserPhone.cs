using System.Text.RegularExpressions;

namespace UserMs.Domain.Entities.IUser.ValueObjects
{
    public partial class UserPhone : ValueObject
    {
        //private const int DefaultLenght = 11;
        private const string Pattern = @"^\d{11}$";

        private UserPhone(string value) => Value = value;

        public UserPhone()
        {

        }
        public static UserPhone Create(string value)
        {
            try
            {
                //if (string.IsNullOrEmpty(value)) throw new NullAtributeException("User phone is required");
                if (!PhoneNumberRegex().IsMatch(value)) throw new InvalidCastException("User phone is invalid");

                return new UserPhone(value);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public string Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        [GeneratedRegex(Pattern)]
        private static partial Regex PhoneNumberRegex();

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }


    }
}
