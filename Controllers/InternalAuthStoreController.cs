using System.Security.Cryptography;
using System.Text;
using Cicadar.Api.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Cicadar.Api.Controllers;

[ApiController]
[Route("internal/auth-store")]
public class InternalAuthStoreController : ControllerBase
{
    private readonly IServiceProvider services;
    private readonly IConfiguration configuration;

    public InternalAuthStoreController(IServiceProvider services, IConfiguration configuration)
    {
        this.services = services;
        this.configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Execute([FromBody] AuthStoreCommand command, CancellationToken cancellationToken)
    {
        if (!Authorized())
        {
            return Unauthorized();
        }

        var store = services.GetService<AuthStore>();
        if (store is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "数据库未配置" });
        }

        try
        {
            var result = await store.Execute(command, cancellationToken);
            return Ok(new { result });
        }
        catch (AuthStoreException error)
        {
            return StatusCode(error.Status, new { message = error.Message });
        }
    }

    [HttpPost("merge-wechat-user")]
    public async Task<IActionResult> MergeWechatUser([FromBody] MergeWechatUserCommand command, CancellationToken cancellationToken)
    {
        if (!Authorized())
        {
            return Unauthorized();
        }

        var store = services.GetService<AuthStore>();
        if (store is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "数据库未配置" });
        }

        try
        {
            await store.MergeWechatUser(command, cancellationToken);
            return Ok(new { result = true });
        }
        catch (AuthStoreException error)
        {
            return StatusCode(error.Status, new { message = error.Message });
        }
    }

    private bool Authorized()
    {
        var expected = configuration["InternalAuth:Token"];
        if (string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }

        if (!Request.Headers.TryGetValue("X-Cicada-Internal-Token", out var providedValues))
        {
            return false;
        }

        var provided = providedValues.ToString();
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
