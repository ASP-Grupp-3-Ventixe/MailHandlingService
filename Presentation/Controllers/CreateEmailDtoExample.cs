using Application.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace Presentation.Controllers;

public class CreateEmailDtoExample : IExamplesProvider<CreateEmailDto>
{
    public CreateEmailDto GetExamples()
    {
        return new CreateEmailDto
        {
            Subject = "Test mail",
            Body = "This is a test email.",
            Recipients =
            [
                new RecipientDto
                {
                    Id = null,
                    Name = "Test User",
                    Email = "user@example.com",
                    RecipientType = "To"
                }
            ],
            LabelIds = [],
            AttachmentIds = []
        };
    }
}