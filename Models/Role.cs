using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using Microsoft.AspNetCore.Identity;

namespace Craidd.Models
{
    public class Role: IdentityRole, IIdentifiable<string>
    {
        [NotMapped]
        public string StringId { get => this.Id; set => Id = value; }
    }
}