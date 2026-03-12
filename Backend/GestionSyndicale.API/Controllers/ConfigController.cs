using Microsoft.AspNetCore.Mvc;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("app-settings")]
    public IActionResult GetAppSettings()
    {
        var config = new
        {
            ResidenceName = _configuration["Syndic:ResidenceName"] ?? "Résidence",
            SyndicLegalName = _configuration["Syndic:LegalName"] ?? "Syndic",
            ResidenceStartYear = int.Parse(_configuration["Syndic:ResidenceStartYear"] ?? DateTime.Now.Year.ToString())
        };

        return Ok(config);
    }
}
