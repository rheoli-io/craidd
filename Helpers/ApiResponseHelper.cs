using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Craidd.Helpers
{
    /// <summary>
    /// Manage JSON Api response
    /// </summary>
    public class ApiResponseHelper : IApiResponseHelper
    {
        public JObject ErrorReponse { get; set; } = new JObject(new JProperty("errors", new JArray()));

        /// <summary>
        /// Create a JSON API Error Response
        /// </summary>
        /// <param name="title"></param>
        /// <param name="detail"></param>
        /// <param name="code"></param>
        /// <param name="sourceParameter"></param>
        /// <returns></returns>
        public ApiResponseHelper AddErrorResponse(string title = null, string detail = null, string code = null, string sourceParameter = null)
        {
            var error = new JObject();

            if (! (title is null) )
            {
                error.Add(new JProperty("title", title));
            }

            if (! (detail is null) )
            {
                error.Add(new JProperty("detail", detail));
            }

            if (! (code is null) )
            {
                error.Add(new JProperty("code", code));
            }

            if (! (sourceParameter is null) )
            {
                error.Add(new JProperty("source", new JObject( new JProperty("parameter", sourceParameter))));
            }

            ErrorReponse["errors"].Value<JArray>().Add(error);

            return this;
        }

        /// <summary>
        /// Parse a ModelState validation into a JSON API Error response
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public ApiResponseHelper ParseModelStateResponse(ModelStateDictionary modelState)
        {
            var errorData = modelState.Where(ms => ms.Value.Errors.Any())
                                      .Select(x => new { x.Key, x.Value.Errors });

            foreach (var stateError in errorData)
            {
                var errorMessages = stateError.Errors;

                foreach (var errorMessage in errorMessages)
                {
                    AddErrorResponse(sourceParameter: stateError.Key, detail: errorMessage.ErrorMessage);
                }
            }

            return this;
        }
    }
}