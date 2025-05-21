using MailHandlingServiceProvider.Business.Extensions;
using MailHandlingServiceProvider.Data.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddContexts(builder.Configuration.GetConnectionString("SqlServer")!);
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddServices(builder.Configuration);

// services.AddSingleton<IEventBus, AzureServiceBusEventBus>();


var app = builder.Build();
app.UseSeedData();


app.MapOpenApi();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MailHandlingService");
    c.RoutePrefix = string.Empty; 
});

app.UseHsts();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

