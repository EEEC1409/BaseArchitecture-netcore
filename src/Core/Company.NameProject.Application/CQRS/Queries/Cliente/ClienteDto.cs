namespace Company.NameProject.Application.CQRS.Queries.Cliente
{
    public record ClienteDto(
        Guid Id,
        string Nombre,
        string Cedula,
        string Email,
        bool Activo);
}
