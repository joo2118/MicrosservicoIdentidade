using MassTransit;
using Identidade.Dominio.Interfaces;
using System.Threading.Tasks;

namespace Identidade.Infraestrutura.RedisNotifier
{
    public class PipelineFilter<T> : IFilter<ConsumerConsumeContext<T>> where T : class
    {
        private readonly IRedisStatusNotifier _statusNotifier;

        public PipelineFilter(IRedisStatusNotifier statusNotifier)
        {
            _statusNotifier = statusNotifier;
        }

        public Task Send(ConsumerConsumeContext<T> context, IPipe<ConsumerConsumeContext<T>> next)
        {
            using (_statusNotifier.SetWorking(context.MessageId?.ToString()))
            {
                next?.Send(context)?.Wait();
                _statusNotifier.SetIdle();
            }

            return Task.CompletedTask;
        }

        public void Probe(ProbeContext context) { }
    }
}
