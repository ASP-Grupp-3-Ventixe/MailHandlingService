using System.Text;
using Application.Extensions;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Mail Handling Service API Documentation",
        Description = "Documentation for the Mail Handling Service API",
    });
    c.ExampleFilters();
    c.EnableAnnotations(); 
    // JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer' [space] and then your token in the text input below.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            []
        }
    });
    
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Services.AddCors(options => { options.AddPolicy("AllowAll",
    policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    }); 
});


builder.Services.AddAuthorization();
builder.Services.AddContexts(builder.Configuration.GetConnectionString("SqlServer")!);
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddServices(builder.Configuration);

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

app.Use(async (context, next) =>
{
    // set a fake user with the same userId as used when creating mail
    var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000000") };
    var identity = new System.Security.Claims.ClaimsIdentity(claims, "Dev");
    context.User = new System.Security.Claims.ClaimsPrincipal(identity);
    await next();
});

app.MapControllers();

app.Run();

