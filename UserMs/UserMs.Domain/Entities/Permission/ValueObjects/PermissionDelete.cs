



namespace UserMs.Domain.Entities
{
    public class PermissionDelete : ValueObject
    {
        public bool Value { get; }

        private PermissionDelete(bool value = false)
        {
            Value = value;
        }

        public static PermissionDelete Create(bool value)
        {
            return new PermissionDelete(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}