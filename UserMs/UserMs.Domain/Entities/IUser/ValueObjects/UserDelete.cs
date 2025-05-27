
namespace UserMs.Domain.Entities
{
    public class UserDelete : ValueObject
    {
        public bool Value { get; }

        private UserDelete(bool value = false)
        {
            Value = value;
        }

        private UserDelete()
        {
           
        }

        public static UserDelete Create(bool value)
        {
            return new UserDelete(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}