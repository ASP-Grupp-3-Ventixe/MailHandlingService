namespace MailHandlingServiceProvider.Business.Handlers;

public interface IFileHandler
{
    Task <string?>  SaveFileAsync(IFormFile? file, string directory);
}