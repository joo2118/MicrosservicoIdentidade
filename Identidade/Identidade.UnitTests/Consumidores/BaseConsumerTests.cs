using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using NSubstitute;
using Identidade.Consumidor.Helpers;
using Xunit;
using Identidade.Consumidor.Consumidores;

namespace Identidade.Consumidor.Tests
{
    public class BaseConsumerTests
    {
        [Fact]
        public async Task Consume_MessageAlreadyConsumed_DoesNotConsumeContext()
        {
            var messageManager = Substitute.For<IMessageManager>();
            var context = new TestContext();
            messageManager.VerifyMessageAlreadyConsumed(Arg.Any<Guid>()).Returns(true);

            var consumer = new TestConsumer(messageManager);

            await consumer.Consume(context);

            await messageManager.DidNotReceive().SaveMessageId(Arg.Any<Guid?>());
        }

        [Fact]
        public async Task Consume_MessageNotConsumed_ConsumesContext()
        {
            var messageManager = Substitute.For<IMessageManager>();
            var context = new TestContext();
            messageManager.VerifyMessageAlreadyConsumed(Arg.Any<Guid>()).Returns(false);

            var consumer = new TestConsumer(messageManager);

            await consumer.Consume(context);

            await messageManager.Received(1).SaveMessageId(Arg.Any<Guid?>());
        }

        private class TestConsumer : ConsumidorBase<TestMessage>
        {
            public TestConsumer(IMessageManager messageManager) : base(messageManager) { }

            public override Task ConsumeContext(ConsumeContext<TestMessage> context)
            {
                return Task.CompletedTask;
            }
        }

        private class TestContext : ConsumeContext<TestMessage>
        {
            public TestMessage Message => throw new NotImplementedException();

            public ReceiveContext ReceiveContext => throw new NotImplementedException();

            public Task ConsumeCompleted => throw new NotImplementedException();

            public IEnumerable<string> SupportedMessageTypes => throw new NotImplementedException();

            public CancellationToken CancellationToken => throw new NotImplementedException();

            public Guid? MessageId => Guid.NewGuid();

            public Guid? RequestId => throw new NotImplementedException();

            public Guid? CorrelationId => throw new NotImplementedException();

            public Guid? ConversationId => throw new NotImplementedException();

            public Guid? InitiatorId => throw new NotImplementedException();

            public DateTime? ExpirationTime => throw new NotImplementedException();

            public Uri SourceAddress => throw new NotImplementedException();

            public Uri DestinationAddress => throw new NotImplementedException();

            public Uri ResponseAddress => throw new NotImplementedException();

            public Uri FaultAddress => throw new NotImplementedException();

            public DateTime? SentTime => throw new NotImplementedException();

            public Headers Headers => throw new NotImplementedException();

            public HostInfo Host => throw new NotImplementedException();

            public SerializerContext SerializerContext => throw new NotImplementedException();

            public void AddConsumeTask(Task task)
            {
                throw new NotImplementedException();
            }

            public T AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory) where T : class
            {
                throw new NotImplementedException();
            }

            public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
            {
                throw new NotImplementedException();
            }

            public ConnectHandle ConnectSendObserver(ISendObserver observer)
            {
                throw new NotImplementedException();
            }

            public T GetOrAddPayload<T>(PayloadFactory<T> payloadFactory) where T : class
            {
                throw new NotImplementedException();
            }

            public Task<ISendEndpoint> GetSendEndpoint(Uri address)
            {
                throw new NotImplementedException();
            }

            public bool HasMessageType(Type messageType)
            {
                throw new NotImplementedException();
            }

            public bool HasPayloadType(Type payloadType)
            {
                throw new NotImplementedException();
            }

            public Task NotifyConsumed(TimeSpan duration, string consumerType)
            {
                throw new NotImplementedException();
            }

            public Task NotifyConsumed<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class
            {
                throw new NotImplementedException();
            }

            public Task NotifyFaulted(TimeSpan duration, string consumerType, Exception exception)
            {
                throw new NotImplementedException();
            }

            public Task NotifyFaulted<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception) where T : class
            {
                throw new NotImplementedException();
            }

            public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class
            {
                throw new NotImplementedException();
            }

            public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
            {
                throw new NotImplementedException();
            }

            public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
            {
                throw new NotImplementedException();
            }

            public Task Publish(object message, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task Publish<T>(object values, CancellationToken cancellationToken = default) where T : class
            {
                throw new NotImplementedException();
            }

            public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
            {
                throw new NotImplementedException();
            }

            public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
            {
                throw new NotImplementedException();
            }

            public void Respond<T>(T message) where T : class
            {
                throw new NotImplementedException();
            }

            public Task RespondAsync<T>(T message) where T : class
            {
                throw new NotImplementedException();
            }

            public Task RespondAsync<T>(T message, IPipe<SendContext<T>> sendPipe) where T : class
            {
                throw new NotImplementedException();
            }

            public Task RespondAsync<T>(T message, IPipe<SendContext> sendPipe) where T : class
            {
                throw new NotImplementedException();
            }

            public Task RespondAsync(object message)
            {
                throw new NotImplementedException();
            }

            public Task RespondAsync(object message, Type messageType)
            {
                throw new NotImplementedException();
            }

            public Task RespondAsync(object message, IPipe<SendContext> sendPipe)
            {
                throw new NotImplementedException();
            }

            public Task RespondAsync(object message, Type messageType, IPipe<SendContext> sendPipe)
            {
                throw new NotImplementedException();
            }

            public Task RespondAsync<T>(object values) where T : class
            {
                throw new NotImplementedException();
            }

            public Task RespondAsync<T>(object values, IPipe<SendContext<T>> sendPipe) where T : class
            {
                throw new NotImplementedException();
            }

            public Task RespondAsync<T>(object values, IPipe<SendContext> sendPipe) where T : class
            {
                throw new NotImplementedException();
            }

            public bool TryGetMessage<T>(out ConsumeContext<T> consumeContext) where T : class
            {
                throw new NotImplementedException();
            }

            public bool TryGetPayload<T>(out T payload) where T : class
            {
                throw new NotImplementedException();
            }
        }

        private class TestMessage { }

    }
}
