using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class CustomerClient
    {
        public string Id { get; set; } = null!;
        public string? CustomerId { get; set; }
        public string? ClientToken { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? IpAddress { get; set; }

        public virtual Customer? Customer { get; set; }
    }
}
