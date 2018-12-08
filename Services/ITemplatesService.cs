using RazorLight;

namespace Craidd.Services
{
    public interface ITemplatesService
    {
        RazorLightEngine engine { get; set; }
    }
}