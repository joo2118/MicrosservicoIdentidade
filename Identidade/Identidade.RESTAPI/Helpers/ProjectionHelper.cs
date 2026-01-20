using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Identidade.RESTAPI.Helpers
{
    internal static class ProjectionHelper
    {
        public static IReadOnlyCollection<T> ApplyProjection<T>(IReadOnlyCollection<T> items, string? projection)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (string.IsNullOrWhiteSpace(projection)) return items;

            var fields = projection
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(f => f.Length > 0)
                .ToArray();

            if (fields.Length == 0) return items;

            var type = typeof(T);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var selected = new List<PropertyInfo>(fields.Length);
            foreach (var f in fields)
            {
                if (props.TryGetValue(f, out var pi))
                    selected.Add(pi);
            }

            if (selected.Count == 0) return items;

            return items.Select(item =>
            {
                if (item == null) return item!;

                var clone = Activator.CreateInstance<T>();
                foreach (var pi in selected)
                {
                    var value = pi.GetValue(item);
                    pi.SetValue(clone, value);
                }
                return clone;
            }).ToArray();
        }
    }
}
