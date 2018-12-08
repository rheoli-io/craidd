using System;
using RazorLight;

namespace Craidd.Services
{
    public class TemplatesService : ITemplatesService
    {
        public RazorLightEngine engine { get; set; }
        public TemplatesService(string templatePath)
        {
            engine = new RazorLightEngineBuilder()
              .UseFilesystemProject(templatePath)
              .UseMemoryCachingProvider()
              .Build();
        }
    }
}