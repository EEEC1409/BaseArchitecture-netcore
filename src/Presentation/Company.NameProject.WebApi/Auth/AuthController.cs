using Company.NameProject.Shared.Exceptions;

using Microsoft.AspNetCore.Mvc;

namespace Company.NameProject.WebApi.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _tokenService;

        public AuthController(IJwtTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Genera un JWT para el usuario autenticado.
        /// Reemplaza la lógica de validación de credenciales con tu repositorio de usuarios.
        /// </summary>
        [HttpPost("login")]
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
