using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFF.ProductCatalog.Events;
internal class VariantCreatedEvent
{
    public Guid VariantId { get; set; }
    public VariantInfo Variant { get; set; } = default!;
}
