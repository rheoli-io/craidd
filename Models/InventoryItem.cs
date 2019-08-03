using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Models;

namespace Craidd.Models
{
    public class InventoryItem : Identifiable
    {
        [Required]
        [Attr("name")]
        public string Name { get; set; }
    }
}