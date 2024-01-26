using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Mastery
    {
        public string Id { get; set; } = null!;
        public string? CategoryId { get; set; }
        public string? StaffId { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Staff? Staff { get; set; }
    }
}
