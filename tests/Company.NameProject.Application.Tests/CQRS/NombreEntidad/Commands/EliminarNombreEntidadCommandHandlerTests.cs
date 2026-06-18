// ============================================================
//  ESQUELETO — Eliminar[NombreEntidad]CommandHandlerTests
//  Reemplazar [NombreEntidad] con el nombre real de la entidad.
// ============================================================

namespace Company.NameProject.Application.Tests.CQRS.NombreEntidad.Commands;

public class EliminarNombreEntidadCommandHandlerTests
{
    // ─── Setup ──────────────────────────────────────────────────────────────
    //
    // private Mock<I[NombreEntidad]Repository> _repositoryMock;
    // private Mock<IUnitOfWork> _unitOfWorkMock;
    // private Eliminar[NombreEntidad]CommandHandler _handler;
    //
    // public EliminarNombreEntidadCommandHandlerTests()
    // {
    //     _repositoryMock = new Mock<I[NombreEntidad]Repository>();
    //     _unitOfWorkMock = new Mock<IUnitOfWork>();
    //     _handler = new Eliminar[NombreEntidad]CommandHandler(
    //         _repositoryMock.Object, _unitOfWorkMock.Object);
    // }

    // ─── Casos de éxito ─────────────────────────────────────────────────────

    [Fact(Skip = "Pendiente de implementar — crear handler primero")]
    public async Task Handle_ConIdExistente_EliminaEntidadYGuardaCambios()
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
