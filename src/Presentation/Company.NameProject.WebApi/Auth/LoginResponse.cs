namespace Company.NameProject.WebApi.Auth
{
    public record LoginResponse(
        string Token,
        string TokenType,
        DateTime Expiration,
        string Username);
}
