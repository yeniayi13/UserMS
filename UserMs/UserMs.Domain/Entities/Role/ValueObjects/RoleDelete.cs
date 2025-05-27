

namespace UserMs.Domain.Entities
{
    public class RoleDelete : ValueObject
    {
        public bool Value { get; }

        private RoleDelete(bool value = false)
        {
            Value = value;
        }

        public static RoleDelete Create(bool value)
        {
            return new RoleDelete(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}