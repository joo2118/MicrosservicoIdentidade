using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Identidade.Consumidor.Helpers
{
    public interface IMessageManager
    {
        Task SaveMessageId(Guid? messageId);
        bool VerifyMessageAlreadyConsumed(Guid? messageId);
    }

    internal class MessageManager : IMessageManager
    {
        private readonly IARCDbContext _arcDbContext;

        public MessageManager(IARCDbContext arcDbContext)
        {
            _arcDbContext = arcDbContext;
        }

        public async Task SaveMessageId(Guid? messageId)
        {
            _arcDbContext.ConsumedMessages.Add(new MessageInformation { MessageId = messageId.ToString() });
            await _arcDbContext.SaveChangesAsync();
        }

        public bool VerifyMessageAlreadyConsumed(Guid? messageId) =>
            _arcDbContext.ConsumedMessages
                .Select(m => m.MessageId)
                .Contains(messageId.ToString());
    }
}
