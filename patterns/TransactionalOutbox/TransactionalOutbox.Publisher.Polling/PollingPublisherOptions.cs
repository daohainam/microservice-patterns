using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionalOutbox.Publisher.Polling
{
    public class PollingPublisherOptions
    {
        public Func<string, Type?> PayloadTypeRsolver { get; set; } = Type.GetType;
    }
}
