using System;
using System.Collections.Generic;

namespace Identidade.Dominio.Extensoes
{
    public static class ExceptionExtensions
    {
        public static IEnumerable<string> GetAllMessages(this Exception e)
        {
            while (e != null)
            {
                yield return e.Message;
                e = e.InnerException;
            }
        }
    }
}
