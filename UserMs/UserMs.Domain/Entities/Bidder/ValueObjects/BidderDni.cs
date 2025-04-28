namespace UserMs.Domain.Entities.Bidder.ValueObjects
{
    public class BidderDni : ValueObject
    {
        private BidderDni(string value) => Value = value;

        public static BidderDni Create(string value)
        {

            //if (string.IsNullOrEmpty(value)) throw new NullAtributeException("UserS address is required");

            return new BidderDni(value);
        }

        public string Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
