using Company.NameProject.Domain.Common;
using FluentAssertions;
using MediatR;

namespace Company.NameProject.Domain.Tests.Common;

public class AggregateRootTests
{
    // ─── Helpers de apoyo ───────────────────────────────────────────────────

    private sealed class TestAggregate : AggregateRoot
    {
        public TestAggregate(Guid id) => Id = id;
        public void Publicar(INotification evento) => AddEvent(evento);
    }

    private sealed class TestAggregateLong : AggregateRootLong
    {
        public TestAggregateLong(long id) => Id = id;
        public void Publicar(INotification evento) => AddEvent(evento);
    }

    private sealed class TestAggregateInt : AggregateRootInt
    {
        public TestAggregateInt(int id) => Id = id;
        public void Publicar(INotification evento) => AddEvent(evento);
    }

    private record TestEvent(string Descripcion) : INotification;

    // ─── DomainEvents (Guid) ────────────────────────────────────────────────

    [Fact]
    public void NuevoAggregateRoot_NoTieneDomainEvents()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddEvent_AgregaEventoALaColeccion()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var evento = new TestEvent("Prueba");

        aggregate.Publicar(evento);

        aggregate.DomainEvents.Should().ContainSingle()
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

        aggregate.DomainEvents.Should().HaveCount(2);
        aggregate.DomainEvents.ElementAt(0).Should().Be(e1);
        aggregate.DomainEvents.ElementAt(1).Should().Be(e2);
    }

    [Fact]
    public void ClearEvents_VaciaLaColeccion()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.Publicar(new TestEvent("Evento"));

        aggregate.ClearEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    // ─── Identidad Guid ─────────────────────────────────────────────────────

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

    // ─── AggregateRootLong (long) ────────────────────────────────────────────

    [Fact]
    public void AggregateRootLong_SoportaIdTipoLong()
    {
        var aggregate = new TestAggregateLong(100L);

        aggregate.Id.Should().Be(100L);
        (aggregate.Id is long).Should().BeTrue();
    }

    [Fact]
    public void AggregateRootLong_DomainEvents_FuncionaIgual()
    {
        var aggregate = new TestAggregateLong(1L);
        aggregate.Publicar(new TestEvent("Long event"));

        aggregate.DomainEvents.Should().ContainSingle();

        aggregate.ClearEvents();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    // ─── AggregateRootInt (int) ──────────────────────────────────────────────

    [Fact]
    public void AggregateRootInt_SoportaIdTipoInt()
    {
        var aggregate = new TestAggregateInt(42);

        aggregate.Id.Should().Be(42);
        (aggregate.Id is int).Should().BeTrue();
    }

    [Fact]
    public void AggregateRootInt_DomainEvents_FuncionaIgual()
    {
        var aggregate = new TestAggregateInt(1);
        aggregate.Publicar(new TestEvent("Int event"));

        aggregate.DomainEvents.Should().ContainSingle();

        aggregate.ClearEvents();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    // ─── IHasDomainEvents (polimorfismo) ────────────────────────────────────

    [Fact]
    public void TodosLosAggregates_ImplementanIHasDomainEvents()
    {
        var guid  = new TestAggregate(Guid.NewGuid());
        var longA = new TestAggregateLong(1L);
        var intA  = new TestAggregateInt(1);

        guid.Should().BeAssignableTo<Domain.Entities.Events.IHasDomainEvents>();
        longA.Should().BeAssignableTo<Domain.Entities.Events.IHasDomainEvents>();
        intA.Should().BeAssignableTo<Domain.Entities.Events.IHasDomainEvents>();
    }
}
