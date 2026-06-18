using Company.NameProject.Domain.Common;
using FluentAssertions;
using MediatR;

namespace Company.NameProject.Domain.Tests.Common;

public class AggregateRootTests
{
    // Clase concreta de apoyo para tests
    private sealed class TestAggregate : AggregateRoot
    {
        public TestAggregate(Guid id) => Id = id;
        public void Publicar(INotification evento) => AddEvent(evento);
    }

    private record TestEvent(string Descripcion) : INotification;

    // ─── Events ─────────────────────────────────────────────────────────────

    [Fact]
    public void NuevoAggregateRoot_NoTieneDomainEvents()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());

        aggregate.Events.Should().BeEmpty();
    }

    [Fact]
    public void AddEvent_AgregaEventoALaColeccion()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var evento = new TestEvent("Prueba");

        aggregate.Publicar(evento);

        aggregate.Events.Should().ContainSingle()
            .Which.Should().Be(evento);
    }

    [Fact]
    public void AddEvent_MultipleEventos_SeAgreganEnOrden()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var e1 = new TestEvent("Primero");
        var e2 = new TestEvent("Segundo");

        aggregate.Publicar(e1);
        aggregate.Publicar(e2);

        aggregate.Events.Should().HaveCount(2);
        aggregate.Events.ElementAt(0).Should().Be(e1);
        aggregate.Events.ElementAt(1).Should().Be(e2);
    }

    [Fact]
    public void ClearEvents_VaciaLaColeccion()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.Publicar(new TestEvent("Evento"));

        aggregate.ClearEvents();

        aggregate.Events.Should().BeEmpty();
    }

    // ─── Identidad (heredada de Entity<Guid>) ───────────────────────────────

    [Fact]
    public void DosAggregates_ConMismoId_SonIguales()
    {
        var id = Guid.NewGuid();
        var a = new TestAggregate(id);
        var b = new TestAggregate(id);

        a.Should().Be(b);
    }

    [Fact]
    public void DosAggregates_ConDistintoId_NoSonIguales()
    {
        var a = new TestAggregate(Guid.NewGuid());
        var b = new TestAggregate(Guid.NewGuid());

        a.Should().NotBe(b);
    }
}
