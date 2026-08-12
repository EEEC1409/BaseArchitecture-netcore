using Microsoft.AspNetCore.Mvc;

namespace Company.NameProject.WebApi.Controllers
{
    /// <summary>
    /// Controlador base del que deben heredar todos los controladores de la API.
    /// </summary>
    /// <remarks>
    /// Expone <see cref="CorrelationToken"/> para que cada acción propague el mismo
    /// <c>X-Correlation-ID</c> del request hacia <c>ApiResponse&lt;T&gt;</c>, garantizando
    /// que token de respuesta, header HTTP y logs de Serilog sean idénticos (ver Sección 2
    /// de la constitución del agente de arquitectura).
    /// </remarks>
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected string CorrelationToken =>
            Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? HttpContext.TraceIdentifier;
    }
}
