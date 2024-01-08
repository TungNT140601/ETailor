using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Role
    {
        public Role()
        {
            Staff = new HashSet<Staff>();
        }

        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public bool? IsDelete { get; set; }

        public virtual ICollection<Staff> Staff { get; set; }
    }
}
