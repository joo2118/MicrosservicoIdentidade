using System;

namespace Identidade.Publico.Compatibility
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CompatibilityStringValueAttribute : Attribute
    {
        public string StringValue { get; }

        public CompatibilityStringValueAttribute( string stringValue )
        {
            StringValue = !string.IsNullOrWhiteSpace(stringValue) ? stringValue : throw new ArgumentException($"{nameof(stringValue)} cannot be null, empty or white-space.", paramName: nameof(stringValue));
        }
    }
}
