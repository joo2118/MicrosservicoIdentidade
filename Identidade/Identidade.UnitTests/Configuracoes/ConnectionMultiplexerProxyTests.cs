using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using Identidade.Infraestrutura.Configuracoes;

namespace Identidade.UnitTests.Configurations
{
    public class ConnectionMultiplexerProxyTests
    {
        public static IEnumerable<object[]> Constructor_Parameters()
        {
            var settings = new Mock<IRedisSettings>();
            var expectedMessageRedisUrl = $"RedisUrl cannot be null, empty or white-space. (Parameter 'redisUrl')";

            yield return new object[] { settings.Object, "localhost" };
            yield return new object[] { null, "localhost", "Value cannot be null. (Parameter 'settings')" };
            yield return new object[] { settings.Object, null, expectedMessageRedisUrl };
            yield return new object[] { settings.Object, string.Empty, expectedMessageRedisUrl };
            yield return new object[] { settings.Object, " ", expectedMessageRedisUrl };
        }

        [Theory]
        [MemberData(nameof(Constructor_Parameters))]
        public void Constructor_Test(IRedisSettings settings, string redisUrl,
            string expectedMessage = null)
        {
            ConnectionMultiplexerProxy Create() => new ConnectionMultiplexerProxy(settings, redisUrl);

            if (string.IsNullOrWhiteSpace(expectedMessage))
            {
                var proxy = Create();

                Assert.NotNull(proxy);
            }
            else
            {
                var ex = Assert.ThrowsAny<ArgumentException>(Create);
                Assert.Equal(expectedMessage, ex.Message);
            }
        }
    }
}
