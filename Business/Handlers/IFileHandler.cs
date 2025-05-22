using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MailHandlingServiceProvider.Business.Handlers;

public interface IFileHandler
{
    Task <string?>  SaveFileAsync(IFormFile? file, string directory);
}