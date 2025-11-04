using MassTransit;
using Identidade.Consumidor.Helpers;
using System.Threading.Tasks;

namespace Identidade.Consumidor.Consumidores
{
    public abstract class ConsumidorBase<T> : IConsumer<T>
        where T : class
    {
        private static object _lockKey = new object();

        private readonly IMessageManager _massageManager;

        public ConsumidorBase(IMessageManager massageManager)
        {
            _massageManager = massageManager;
        }

        public async Task Consume(ConsumeContext<T> context)
        {
            lock (_lockKey)
            {
                if (_massageManager.VerifyMessageAlreadyConsumed(context.MessageId))
                    return;

                _massageManager.SaveMessageId(context.MessageId).Wait();
            }
            await ConsumeContext(context);
        }

        public abstract Task ConsumeContext(ConsumeContext<T> context);
    }
}
