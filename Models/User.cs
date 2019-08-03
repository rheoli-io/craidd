using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Models;
using Microsoft.AspNetCore.Identity;

namespace Craidd.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class User : IdentityUser, IIdentifiable<string>
    {
        [NotMapped]
        public string StringId { get => this.Id; set => Id = value; }
    }
}
