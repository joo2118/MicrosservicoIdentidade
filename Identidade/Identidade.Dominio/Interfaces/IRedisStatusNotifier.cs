using System;

namespace Identidade.Dominio.Interfaces
{
    public interface IRedisStatusNotifier
    {
        void SetIdle();
        IDisposable SetWorking(string strMsgId);
        void SetStarting();
    }
}
