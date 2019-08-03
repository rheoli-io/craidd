using System.Threading.Tasks;
using Craidd.Models;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craidd.Controllers
{
    public class InventoryItemsController : JsonApiController<InventoryItem>
    {
        public InventoryItemsController(
            IJsonApiContext jsonApiContext,
            IResourceService<InventoryItem> resourceService,
            ILoggerFactory loggerFactory
        ) : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }
}