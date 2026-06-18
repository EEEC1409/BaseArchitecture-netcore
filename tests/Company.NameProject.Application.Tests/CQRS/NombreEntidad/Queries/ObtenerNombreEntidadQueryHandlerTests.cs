// ============================================================
//  ESQUELETO — Obtener[NombreEntidad]QueryHandlerTests
//  Caso de uso: obtener una entidad por su Id.
//  Reemplazar [NombreEntidad] con el nombre real de la entidad.
// ============================================================

namespace Company.NameProject.Application.Tests.CQRS.NombreEntidad.Queries;

public class ObtenerNombreEntidadQueryHandlerTests
{
    // ─── Setup ──────────────────────────────────────────────────────────────
    //
    // private Mock<I[NombreEntidad]Repository> _repositoryMock;
    // private Obtener[NombreEntidad]QueryHandler _handler;
    //
    // public ObtenerNombreEntidadQueryHandlerTests()
    // {
    //     _repositoryMock = new Mock<I[NombreEntidad]Repository>();
    //     _handler = new Obtener[NombreEntidad]QueryHandler(_repositoryMock.Object);
    // }

    // ─── Casos de éxito ─────────────────────────────────────────────────────

    [Fact(Skip = "Pendiente de implementar — crear handler primero")]
    public async Task Handle_ConIdExistente_RetornaDto()
    {
        // Arrange

        // Act

        // Assert
    }

    // ─── Casos de error ─────────────────────────────────────────────────────

    [Fact(Skip = "Pendiente de implementar — crear handler primero")]
    public async Task Handle_ConIdNoExistente_LanzaApiExceptionNotFound()
    {
        // Arrange

        // Act

        // Assert
    }
}
