namespace UserMs.Domain.Entities.IUser.ValueObjects
{
    public class UserAddress: ValueObject
    {
        private UserAddress(string value) => Value = value;


        public UserAddress()
        {

        }
        public static UserAddress Create(string value)
        {

           // if (string.IsNullOrEmpty(value)) throw new NullAtributeException("UserS address is required");

            return new UserAddress(value);
        }

        public string Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
