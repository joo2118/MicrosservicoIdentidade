using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Identidade.UnitTests.Helpers
{
    public interface IGetParametersSynthax
    {
        IGetParametersSynthax AddParameter<T>(string name, T[] correctValues, T[] wrongValues);
        IGetParametersSynthax AddStringParameter(string name, params string?[] values);
        IGetParametersSynthax AddNonNullableParameter<T>(string name, params T[] values) where T : notnull;
        IGetParametersSynthax AddOptionalNonNullableParameter<T>(string name, params T[] values) where T : notnull;
        IGetParametersSynthax AddNullableParameter<T>(string name, params T[] values) where T : class?;
    }

    public static class ParameterTestHelper
    {
        public static IEnumerable<object?[]> GetParameters(Action<IGetParametersSynthax> s)
        {
            var builder = new GetParametersBuilder();

            s(builder);

            return GetCorrectParameterValues(builder).Concat(GetWrongParameterValues(builder));
        }

        public static IEnumerable<object?[]> GetWrongParameters(Action<IGetParametersSynthax> s)
        {
            var builder = new GetParametersBuilder();

            s(builder);

            return GetWrongParameterValues(builder);
        }

        private static IEnumerable<object?[]> GetCorrectParameterValues(GetParametersBuilder builder)
        {
            yield return builder.Parameters.Select(p => p.CorrectValues.First()).ToArray();

            foreach (var p in builder.Parameters.Where(p => p.CorrectValues.Length >= 2))
                foreach (var v in p.CorrectValues.Skip(1))
                    yield return GetParameterValues(builder, p.Name, v).ToArray();
        }

        private static IEnumerable<object?[]> GetWrongParameterValues(GetParametersBuilder builder)
        {
            foreach (var p in builder.Parameters.Where(p => p.WrongValues.Any()))
                foreach (var v in p.WrongValues)
                    yield return GetParameterValues(builder, p.Name, v).Append(p.Name).ToArray();
        }

        private static IEnumerable<object?> GetParameterValues(GetParametersBuilder builder, string parameterName, object? parameterValue)
        {
            foreach (var p in builder.Parameters)
            {
                if (p.Name == parameterName)
                    yield return parameterValue;
                else
                    yield return p.CorrectValues.First();
            }
        }

        private class GetParametersBuilder : IGetParametersSynthax
        {
            public List<ParameterInfo> Parameters { get; }

            public GetParametersBuilder()
            {
                Parameters = new List<ParameterInfo>();
            }

            public IGetParametersSynthax AddParameter<T>(string name, T[] correctValues, T[] wrongValues)
            {
                Parameters.Add(new ParameterInfo(name, correctValues.Cast<object?>().ToArray(), wrongValues.Cast<object?>().ToArray()));

                return this;
            }

            public IGetParametersSynthax AddStringParameter(string name, params string?[] values) =>
                AddParameter(name, values, values.Any(string.IsNullOrWhiteSpace) ? Array.Empty<string?>() : new string?[] { null, string.Empty, " " });

            public IGetParametersSynthax AddNonNullableParameter<T>(string name, params T[] values) where T : notnull =>
                AddParameter(name, values, Array.Empty<T>());

            public IGetParametersSynthax AddOptionalNonNullableParameter<T>(string name, params T[] values) where T : notnull =>
                AddParameter(name, values.Cast<object?>().Append(null).ToArray(), Array.Empty<object?>());

            public IGetParametersSynthax AddNullableParameter<T>(string name, params T[] values) where T : class? =>
                AddParameter(name, values, values.Any(t => t == null) ? Array.Empty<T>() : new T[] { null });
        }

        private class ParameterInfo
        {
            public string Name { get; }
            public object?[] CorrectValues { get; }
            public object?[] WrongValues { get; }

            public ParameterInfo(string name, object?[] correctValues, object?[] wrongValues)
            {
                Name = name;
                CorrectValues = correctValues;
                WrongValues = wrongValues;
            }
        }
    }

    public class ParameterTestHelperTests
    {
        [Fact]
        public void ParameterTestHelperGetParametersTest()
        {
            var actual = ParameterTestHelper.GetParameters(s => s
                .AddNonNullableParameter("p1", 1)
                .AddStringParameter("p2", "v2")
                .AddNullableParameter<int[]>("p3", new int[] { 3 })
                .AddParameter("p4",
                    correctValues: new Type[] { typeof(string) },
                    wrongValues: new Type[] { typeof(DateTime) }))
                .ToArray();

            Assert.Equal(6, actual.Length);

            Assert.Equal(new object?[] { 1, "v2", new int[] { 3 }, typeof(string) }, actual[0]);
            Assert.Equal(new object?[] { 1, null, new int[] { 3 }, typeof(string), "p2" }, actual[1]);
            Assert.Equal(new object?[] { 1, string.Empty, new int[] { 3 }, typeof(string), "p2" }, actual[2]);
            Assert.Equal(new object?[] { 1, " ", new int[] { 3 }, typeof(string), "p2" }, actual[3]);
            Assert.Equal(new object?[] { 1, "v2", null, typeof(string), "p3" }, actual[4]);
            Assert.Equal(new object?[] { 1, "v2", new int[] { 3 }, typeof(DateTime), "p4" }, actual[5]);
        }

        [Fact]
        public void ParameterTestHelperGetWrongParametersTest()
        {
            var actual = ParameterTestHelper.GetWrongParameters(s => s
                .AddNonNullableParameter("p1", 1)
                .AddStringParameter("p2", "v2")
                .AddNullableParameter<int[]>("p3", new int[] { 3 })
                .AddParameter("p4",
                    correctValues: new Type[] { typeof(string) },
                    wrongValues: new Type[] { typeof(DateTime) }))
                .ToArray();

            Assert.Equal(5, actual.Length);

            Assert.Equal(new object?[] { 1, null, new int[] { 3 }, typeof(string), "p2" }, actual[0]);
            Assert.Equal(new object?[] { 1, string.Empty, new int[] { 3 }, typeof(string), "p2" }, actual[1]);
            Assert.Equal(new object?[] { 1, " ", new int[] { 3 }, typeof(string), "p2" }, actual[2]);
            Assert.Equal(new object?[] { 1, "v2", null, typeof(string), "p3" }, actual[3]);
            Assert.Equal(new object?[] { 1, "v2", new int[] { 3 }, typeof(DateTime), "p4" }, actual[4]);
        }
    }
}
