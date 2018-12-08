using System.Collections.Generic;
using System.Threading.Tasks;

namespace Craidd.Services
{
    public interface IEmailsService
    {
        Task<bool> sendEmailFromTemplateAsync(string email, string subject, string templateFile, Dictionary<string, object> messageData);
    }
}