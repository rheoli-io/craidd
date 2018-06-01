

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace Craidd.Helpers
{
    public interface IApiResponseHelper
    {
        JObject ErrorReponse { get; set; }
        ApiResponseHelper AddErrorResponse(string title = null, string detail = null, string code = null, string sourceParameter = null);
        ApiResponseHelper ParseModelStateResponse(ModelStateDictionary modelState);
    }

}