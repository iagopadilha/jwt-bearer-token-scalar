using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt; // Adicione este using


namespace ScalarProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration _config; // Adicione este campo

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config; // Atribua o parâmetro ao campo
        }

        // para documentar


        [HttpGet(Name = "GetWeatherForecast")]
        [EndpointSummary("Get the weather forecast for the next 5 days")]
        [EndpointDescription("This is a Wheather Route to get the weather. It may be cold or hot")]
        [ProducesResponseType(200)]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        [HttpGet("private/{number:int}")]
        [Authorize] // necessario para utilizar do bearer token corretamente
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult GetPrivate(int number)
        {
            return number > 6 ? Ok(number) : BadRequest(number);
        }


        [HttpGet("getToken")]
        [ProducesResponseType(200)]
        [AllowAnonymous]
        public IActionResult GenerateToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Iago"),
                new Claim(ClaimTypes.Email, "iago@iago"),
            };
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }

    }
}
