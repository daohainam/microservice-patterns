using EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Abstractions
{
    public class MessageEnvelop
    {
        public MessageEnvelop()
        {
        }
        public MessageEnvelop(Type type, string message): this(type.FullName!, message)
        {
        }

        public MessageEnvelop(string messageTypeName, string message)
        {
            MessageTypeName = messageTypeName ?? throw new ArgumentNullException(nameof(messageTypeName));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string MessageTypeName { get; set; } = default!;
        public string Message { get; set; } = default!;
    }
}
