using System;

namespace Identidade.Dominio.Helpers
{
    public static class ExceptionCatcher
    {
        public static TResponse ExecuteSafe<TException, TResponse>(Func<TResponse> safeFunction,
            Action<TException> onCatchException) where TException : AppException
        {
            try
            {
                return safeFunction();
            }
            catch (TException e)
            {
                onCatchException(e);
                return default(TResponse);
            }
        }

        public static void ExecuteSafe<TException>(Action safeAction,
            Action<TException> onCatchException) where TException : AppException
        {
            try
            {
                safeAction();
            }
            catch (TException e)
            {
                onCatchException(e);
            }
        }
    }
}
