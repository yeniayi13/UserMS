namespace UserMs.Domain.Entities.Bidder.ValueObjects
{
    public class BidderId : ValueObject
    {
        private BidderId(Guid value) => Value = value;

        public static BidderId Create(Guid value)
        {

            return new BidderId(value);
        }

        public Guid Value { get; init; } //*init no permite setear desde afuera, solo desde el constructor

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

    }
}
