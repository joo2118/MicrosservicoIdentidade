using Identidade.Publico.Dtos;
using System;

namespace Identidade.Publico.Events
{
    public class HealthCheckIdentityEvent
    {
        public Guid Id { get; }
        public HealthCheckValuesDto Values { get; }
        public string ErrorMessage { get; }

        public HealthCheckIdentityEvent(Guid id, HealthCheckValuesDto values, string errorMessage)
        {
            if (values is null && string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentException($"One of either {nameof(values)} or {nameof(errorMessage)} is required.", nameof(values));

            if (values != null && !string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentException($"{nameof(values)} and {nameof(errorMessage)} cannot be used simultaneously.", nameof(values));

            Id = id;
            Values = values;
            ErrorMessage = errorMessage;
        }

        public static HealthCheckIdentityEvent FromValues(Guid id, HealthCheckValuesDto values) =>
            new HealthCheckIdentityEvent(id, values, null);

        public static HealthCheckIdentityEvent FromErrorMessage(Guid id, string errorMessage) =>
            new HealthCheckIdentityEvent(id, null, errorMessage);
    }
}
