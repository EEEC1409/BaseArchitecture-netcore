namespace Company.NameProject.Shared.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public List<string> Errors { get; } = new();

        public ApiException(string message, int statusCode = 400)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(List<string> errors, int statusCode = 400)
            : base(string.Join("; ", errors))
        {
            StatusCode = statusCode;
            Errors = errors;
        }

        public static ApiException NotFound(string resource) =>
            new($"{resource} no encontrado.", 404);

        public static ApiException Unauthorized(string message = "No autorizado.") =>
            new(message, 401);

        public static ApiException Forbidden(string message = "Acceso denegado.") =>
            new(message, 403);
    }
}
