using System;
using System.Threading.Tasks;

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
                return default;
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

        public static async Task<TResponse> ExecuteSafeAsync<TException, TResponse>(Func<Task<TResponse>> safeFunction,
            Func<TException, Task> onCatchException) where TException : AppException
        {
            try
            {
                return await safeFunction().ConfigureAwait(false);
            }
            catch (TException e)
            {
                await onCatchException(e).ConfigureAwait(false);
                return default(TResponse);
            }
        }

        public static async Task ExecuteSafeAsync<TException>(Func<Task> safeAction,
            Func<TException, Task> onCatchException) where TException : AppException
        {
            try
            {
                await safeAction().ConfigureAwait(false);
            }
            catch (TException e)
            {
                await onCatchException(e).ConfigureAwait(false);
            }
        }
    }
}