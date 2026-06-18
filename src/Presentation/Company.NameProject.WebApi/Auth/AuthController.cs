using Company.NameProject.Shared.Exceptions;

using Microsoft.AspNetCore.Mvc;

namespace Company.NameProject.WebApi.Auth
{
    /// <summary>
    /// Gestiona la autenticación de usuarios y la emisión de tokens JWT.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _tokenService;

        public AuthController(IJwtTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Autentica al usuario y devuelve un token JWT Bearer.
        /// </summary>
        /// <remarks>
        /// **Credenciales de demo:**
        ///
        ///     Admin → username: admin   | password: Admin123!
        ///     User  → username: user    | password: User123!
        ///
        /// Reemplaza <c>IsValidUser</c> con la validación real contra tu repositorio de usuarios.
        /// </remarks>
        /// <param name="request">Credenciales del usuario.</param>
        /// <returns>Token JWT Bearer listo para usar en el header <c>Authorization</c>.</returns>
        /// <response code="200">Login exitoso. Retorna el token JWT.</response>
        /// <response code="401">Credenciales inválidas.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // TODO: Reemplazar con validación real contra base de datos
            if (!IsValidUser(request.Username, request.Password, out var roles))
                throw ApiException.Unauthorized("Credenciales inválidas.");

            var response = _tokenService.GenerateToken(request.Username, roles);

            return Ok(ApiResponse<LoginResponse>.Success(response, "Login exitoso."));
        }

        private static bool IsValidUser(string username, string password, out List<string> roles)
        {
            roles = new List<string>();

            // Demo: reemplazar con validación real
            if (username == "admin" && password == "Admin123!")
            {
                roles = ["Admin", "User"];
                return true;
            }

            if (username == "user" && password == "User123!")
            {
                roles = ["User"];
                return true;
            }

            return false;
        }
    }
}
