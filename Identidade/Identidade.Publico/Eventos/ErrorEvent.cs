using System;
using System.Collections.Generic;

namespace Identidade.Publico.Eventos
{

    public class ErrorEvent
    {
        public string Consumer { get; set; }
        public string Action { get; set; }
        public string MessageType { get; set; }
        public string MessageId { get; set; }
        public string CorrelationId { get; set; }
        public string ConversationId { get; set; }

        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }

        public DateTimeOffset OccurredAtUtc { get; set; } = DateTimeOffset.UtcNow;

        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}