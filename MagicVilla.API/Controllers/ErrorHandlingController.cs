using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers;
[Route("ErrorHandling")]
[ApiController]
[AllowAnonymous]
[ApiVersionNeutral]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorHandlingController : ControllerBase
{
    [Route("ProcessError")]
    public IActionResult ProcessError([FromServices] IHostEnvironment hostEnvironment)
    {
        //Custom Logic
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (hostEnvironment.IsDevelopment())
        {

            return Problem
                (
                    detail: feature.Error.StackTrace,
                    title: feature.Error.Message,
                    instance: hostEnvironment.EnvironmentName
                );
        }
        else
        {
            return Problem();
        }
    }

}
