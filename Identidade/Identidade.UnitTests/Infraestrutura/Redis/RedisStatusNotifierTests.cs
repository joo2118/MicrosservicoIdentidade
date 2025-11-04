using Moq;
using Identidade.Infraestrutura.RedisNotifier;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Xunit;
using Identidade.Infraestrutura.Configuracoes;

namespace Identidade.UnitTests.Infraestrutura.RedisNotifier
{
    public class RedisStatusNotifierTests
    {
        public static IEnumerable<object[]> Constructor_Parameters()
        {
            var timer = new Timer();
            var mockConnectionMultiplexerProx = new Mock<IConnectionMultiplexerProxy>();
            var applicationIdentifier = "applicationIdentifier";
            var applicationId = "applicationId";

            yield return new object[] { timer, mockConnectionMultiplexerProx.Object, applicationIdentifier, applicationId };

            yield return new object[] { timer, null, applicationIdentifier, applicationId, "connection" };
            yield return new object[] { null, mockConnectionMultiplexerProx.Object, applicationIdentifier, applicationId, "timer" };

            yield return new object[] { timer, mockConnectionMultiplexerProx.Object, null, applicationId, "applicationIdentifier" };
            yield return new object[] { timer, mockConnectionMultiplexerProx.Object, string.Empty, applicationId, "applicationIdentifier" };
            yield return new object[] { timer, mockConnectionMultiplexerProx.Object, " ", applicationId, "applicationIdentifier" };

            yield return new object[] { timer, mockConnectionMultiplexerProx.Object, applicationIdentifier, null, "applicationId" };
            yield return new object[] { timer, mockConnectionMultiplexerProx.Object, applicationIdentifier, string.Empty, "applicationId" };
            yield return new object[] { timer, mockConnectionMultiplexerProx.Object, applicationIdentifier, " ", "applicationId" };

        }

        [Theory]
        [MemberData(nameof(Constructor_Parameters))]
        public void Constructor_Test(Timer timer, IConnectionMultiplexerProxy connection, string applicationIdentifier, string applicationId,
            string missingParameterName = null)
        {
            RedisStatusNotifier Create() => new RedisStatusNotifier(timer, connection, applicationIdentifier, applicationId);

            if (string.IsNullOrWhiteSpace(missingParameterName))
            {
                var notifier = Create();

                Assert.NotNull(notifier);
            }
            else
            {
                var ex = Assert.ThrowsAny<ArgumentException>(Create);
                Assert.Equal(missingParameterName, ex.ParamName);
            }
        }

        [Fact]
        public void SetWorking_SetsCorrectValues_Test()
        {
            var msgId = "testMsgId";
            var msgKey = string.Concat(RedisConstants.Path.REDIS_MESSAGE_ACTIVE, msgId);
            var expectedRedisTime = 1739390240L;
            var expectedProcessId = string.Concat(Environment.ProcessId, '@', Environment.MachineName);

            var timer = new Timer(10000);
            var mockConnectionMultiplexerProx = new Mock<IConnectionMultiplexerProxy>();

            var notifier = new RedisStatusNotifier(timer, mockConnectionMultiplexerProx.Object, "applicationIdentifier", "applicationId");

            mockConnectionMultiplexerProx.Setup(c => c.GetRedisTime()).Returns(expectedRedisTime);

            var disposable = notifier.SetWorking(msgId);

            mockConnectionMultiplexerProx.Verify(c => c.SetKey(
                It.Is<RedisKey>(key => key.Equals(msgKey)),
                It.Is<HashEntry[]>(entries =>
                    entries.Length == 3 &&
                    entries[0].Name == RedisConstants.Field.REDIS_FIELD_STATUS && entries[0].Value == RedisConstants.Status.REDIS_STATUS_WORKING &&
                    entries[1].Name == RedisConstants.Field.REDIS_FIELD_STATUS_TIME && (long)entries[1].Value == expectedRedisTime &&
                    entries[2].Name == "applicationId" && entries[2].Value == expectedProcessId
                ),
                It.IsAny<CommandFlags>()
            ), Times.Once);

            disposable.Dispose();

            mockConnectionMultiplexerProx.Verify(s => s.GetRedisTime(), Times.Once);
            mockConnectionMultiplexerProx.Verify(s => s.SetKeyExpiration(msgKey, null, CommandFlags.None), Times.Once);
            mockConnectionMultiplexerProx.Verify(s => s.DeleteKey(msgKey, CommandFlags.None), Times.Once);
        }

        [Fact]
        public void SetIdle_SetsCorrectValues_Test()
        {
            var applicationIdentifier = "applicationIdentifier";
            var msgKey = string.Concat(applicationIdentifier, Environment.ProcessId, '@', Environment.MachineName);
            var expectedRedisTime = 1739390240L;
            var expectedProcessId = string.Concat(Environment.ProcessId, '@', Environment.MachineName);

            var timer = new Timer(10000);
            var mockConnectionMultiplexerProx = new Mock<IConnectionMultiplexerProxy>();

            var notifier = new RedisStatusNotifier(timer, mockConnectionMultiplexerProx.Object, applicationIdentifier, "applicationId");

            mockConnectionMultiplexerProx.Setup(c => c.GetRedisTime()).Returns(expectedRedisTime);

            notifier.SetIdle();

            mockConnectionMultiplexerProx.Verify(c => c.SetKey(
                It.Is<RedisKey>(key => key.Equals(msgKey)),
                It.Is<HashEntry[]>(entries =>
                    entries.Length == 3 &&
                    entries[0].Name == RedisConstants.Field.REDIS_FIELD_MACHINE && entries[0].Value == Environment.MachineName &&
                    entries[1].Name == RedisConstants.Field.REDIS_FIELD_STATUS && entries[1].Value == RedisConstants.Status.REDIS_STATUS_IDLE &&
                    entries[2].Name == RedisConstants.Field.REDIS_FIELD_STATUS_TIME && (long)entries[2].Value == expectedRedisTime
                ),
                It.IsAny<CommandFlags>()
            ), Times.Once);

            mockConnectionMultiplexerProx.Verify(s => s.GetRedisTime(), Times.Once);
            mockConnectionMultiplexerProx.Verify(s => s.SetKeyExpiration(msgKey, null, CommandFlags.None), Times.Once);
            mockConnectionMultiplexerProx.Verify(s => s.DeleteKey(msgKey, CommandFlags.None), Times.Never);
        }

        [Fact]
        public void SetStarting_SetsCorrectValues_Test()
        {
            var applicationIdentifier = "applicationIdentifier";
            var msgKey = string.Concat(applicationIdentifier, Environment.ProcessId, '@', Environment.MachineName);
            var expectedRedisTime = 1739390240L;
            var expectedProcessId = string.Concat(Environment.ProcessId, '@', Environment.MachineName);

            var timer = new Timer(10000);
            var mockConnectionMultiplexerProx = new Mock<IConnectionMultiplexerProxy>();

            var notifier = new RedisStatusNotifier(timer, mockConnectionMultiplexerProx.Object, applicationIdentifier, "applicationId");

            mockConnectionMultiplexerProx.Setup(c => c.GetRedisTime()).Returns(expectedRedisTime);

            notifier.SetStarting();

            mockConnectionMultiplexerProx.Verify(c => c.SetKey(
                It.Is<RedisKey>(key => key.Equals(msgKey)),
                It.Is<HashEntry[]>(entries =>
                    entries.Length == 3 &&
                    entries[0].Name == RedisConstants.Field.REDIS_FIELD_MACHINE && entries[0].Value == Environment.MachineName &&
                    entries[1].Name == RedisConstants.Field.REDIS_FIELD_STATUS && entries[1].Value == RedisConstants.Status.REDIS_STATUS_STARTING &&
                    entries[2].Name == RedisConstants.Field.REDIS_FIELD_STATUS_TIME && (long)entries[2].Value == expectedRedisTime
                ),
                It.IsAny<CommandFlags>()
            ), Times.Once);

            mockConnectionMultiplexerProx.Verify(s => s.GetRedisTime(), Times.Once);
            mockConnectionMultiplexerProx.Verify(s => s.SetKeyExpiration(msgKey, null, CommandFlags.None), Times.Once);
            mockConnectionMultiplexerProx.Verify(s => s.DeleteKey(msgKey, CommandFlags.None), Times.Never);
        }

        [Fact]
        public async Task SetLastAlive_SetsCorrectValues_Test()
        {
            var applicationIdentifier = "applicationIdentifier";
            var consumerId = string.Concat(Environment.ProcessId, '@', Environment.MachineName);
            var msgKey = string.Concat(applicationIdentifier, consumerId);
            var expectedRedisTime = 1739390240L;

            var timer = new Timer(100);
            var mockConnectionMultiplexerProx = new Mock<IConnectionMultiplexerProxy>();

            var notifier = new RedisStatusNotifier(timer, mockConnectionMultiplexerProx.Object, applicationIdentifier, "applicationId");

            mockConnectionMultiplexerProx.Setup(c => c.GetRedisTime()).Returns(expectedRedisTime);

            timer.Start();

            await Task.Delay(1001);

            mockConnectionMultiplexerProx.Verify(c => c.SetKey(
                It.Is<RedisKey>(key => key.Equals(msgKey)),
                It.Is<HashEntry[]>(entries =>
                    entries.Length == 2 &&
                    entries[0].Name == RedisConstants.Field.REDIS_FIELD_MACHINE && entries[0].Value == Environment.MachineName &&
                    entries[1].Name == RedisConstants.Field.REDIS_FIELD_LASTALIVE && (long)entries[1].Value == expectedRedisTime
                ),
                It.IsAny<CommandFlags>()
            ), Times.AtLeastOnce);

            mockConnectionMultiplexerProx.Verify(s => s.GetRedisTime(), Times.AtLeastOnce);
            mockConnectionMultiplexerProx.Verify(s => s.SetKeyExpiration(consumerId, null, CommandFlags.FireAndForget), Times.AtLeastOnce);
        }
    }
}
