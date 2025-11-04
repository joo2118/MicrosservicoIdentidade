using System;
using System.Collections.Generic;

namespace Identidade.Dominio.Helpers
{
    public class AppException : Exception
    {
        /// <summary>
        /// An array containing the error messages with the correct format to be displayed for the user.
        /// </summary>
        public IReadOnlyCollection<string> Errors { get; }

        public AppException() { }

        public AppException(params string[] errors)
            : base(string.Join(";", errors))
        {
            Errors = errors;
        }
    }

    public class ConflictAppException : AppException
    {
        public ConflictAppException() { }

        public ConflictAppException(params string[] errors)
            : base(errors) { }
    }

    public class NotFoundAppException : AppException
    {
        public NotFoundAppException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Identidade.Dominio.Helpers.NotFoundAppException"></see>
        /// class with the message "There's no {entityName} with the {propertyName} '{propertyValue}' on the database."
        /// </summary>
        public NotFoundAppException(string entityName, string propertyName, string propertyValue)
            : base($"There's no {entityName} with the {propertyName} '{propertyValue}' on the database.") { }

        public NotFoundAppException(params string[] errors)
            : base(errors) { }
    }

    public class MessageAlreadyConsumedAppException : AppException
    {
        public MessageAlreadyConsumedAppException()
            : base("This message was already consumed") { }
    }
}
