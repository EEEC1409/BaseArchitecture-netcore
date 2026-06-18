using Company.NameProject.Application.Common.Behaviors;
using Company.NameProject.Application.Common.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace Company.NameProject.Application.Tests.Common.Behaviors;

public class TransactionBehaviorTests
{
    public record TestTransactionCommand : IRequest<string>, IRequiresTransaction;

    private static RequestHandlerDelegate<string> NextHandler(string resultado = "ok")
        => _ => Task.FromResult(resultado);

    private static RequestHandlerDelegate<string> NextHandlerQueExplota()
        => _ => throw new InvalidOperationException("Error en handler");

    // ─── Flujo exitoso ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_HandlerExitoso_BeginYCommitTransaction()
    {
        var uowMock = new Mock<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestTransactionCommand, string>(uowMock.Object);

        var result = await behavior.Handle(
            new TestTransactionCommand(), NextHandler("ok"), CancellationToken.None);

        result.Should().Be("ok");
        uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─── Flujo con excepción ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_HandlerLanzaExcepcion_BeginYRollbackTransaction()
    {
        var uowMock = new Mock<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestTransactionCommand, string>(uowMock.Object);

        var act = async () => await behavior.Handle(
            new TestTransactionCommand(), NextHandlerQueExplota(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_HandlerLanzaExcepcion_RelanzaLaExcepcion()
    {
        var uowMock = new Mock<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestTransactionCommand, string>(uowMock.Object);

        var act = async () => await behavior.Handle(
            new TestTransactionCommand(), NextHandlerQueExplota(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Error en handler");
    }
}
