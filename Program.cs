using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
    document.Info = new()
    {
        Title = "Scalar Project API",
        Version = "1.0.0",
        Description = "This is a sample API for Scalar Project using .NET 9.0 and C# 13.0."
    };
    document.Info.Contact = new()
    {
        Name = "Scalar Project Team",
        Email = "iago@treste",
        Url = new Uri("https://scalarproject.com")
        };
        return Task.CompletedTask;
    });
});

string scheme = JwtBearerDefaults.AuthenticationScheme;
builder.Services.AddAuthentication(scheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapScalarApiReference( options =>
{
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    // Torna o BearerAuth o esquema preferido (default)
    options.AddPreferredSecuritySchemes("BearerAuth");
});
app.MapControllers();

app.Run();

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) :
    IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authscheme = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authscheme.Any(authScheme => authScheme.Name == JwtBearerDefaults.AuthenticationScheme))
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Insira o token JWT diretamente para poder utilizar as rotas: {token}"
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

            // Use "BearerAuth" como chave
            document.Components.SecuritySchemes["BearerAuth"] = securityScheme;

            document.SecurityRequirements = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [securityScheme] = new List<string>()
                }
            };
        }
    }
}


