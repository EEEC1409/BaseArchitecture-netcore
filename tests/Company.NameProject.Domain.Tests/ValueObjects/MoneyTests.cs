using Company.NameProject.Domain.Common;
using Company.NameProject.Domain.ValueObjects;
using FluentAssertions;

namespace Company.NameProject.Domain.Tests.ValueObjects;

public class MoneyTests
{
    // ─── Crear — casos válidos ──────────────────────────────────────────────

    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var money = Money.Crear(100m, "usd");

        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Crear_NormalizaMonedaAMayusculas()
    {
        var money = Money.Crear(50m, "pen");

        money.Currency.Should().Be("PEN");
    }

    [Fact]
    public void Crear_ConMontoEnCero_EsValido()
    {
        var act = () => Money.Crear(0m, "USD");

        act.Should().NotThrow();
    }

    // ─── Crear — casos inválidos ────────────────────────────────────────────

    [Fact]
    public void Crear_ConMontoNegativo_LanzaDomainException()
    {
        var act = () => Money.Crear(-1m, "USD");

        act.Should().Throw<DomainException>().WithMessage("Monto inválido");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConMonedaVaciaONula_LanzaDomainException(string? moneda)
    {
        var act = () => Money.Crear(10m, moneda!);

        act.Should().Throw<DomainException>().WithMessage("Moneda requerida");
    }

    // ─── Sumar ──────────────────────────────────────────────────────────────

    [Fact]
    public void Sumar_ConMismaMoneda_RetornaNuevoMoney()
    {
        var a = Money.Crear(100m, "USD");
        var b = Money.Crear(50m, "USD");

        var result = a.Sumar(b);

        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Sumar_ConDistintaMoneda_LanzaDomainException()
    {
        var a = Money.Crear(100m, "USD");
        var b = Money.Crear(50m, "PEN");

        var act = () => a.Sumar(b);

        act.Should().Throw<DomainException>().WithMessage("Moneda distinta");
    }

    // ─── Igualdad ───────────────────────────────────────────────────────────

    [Fact]
    public void DosMoneys_ConMismosValores_SonIguales()
    {
        var a = Money.Crear(100m, "USD");
        var b = Money.Crear(100m, "USD");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void DosMoneys_ConDistintaMoneda_NoSonIguales()
    {
        var a = Money.Crear(100m, "USD");
        var b = Money.Crear(100m, "PEN");

        (a != b).Should().BeTrue();
    }

    [Fact]
    public void ToString_RetornaFormato_Monto_Moneda()
    {
        var money = Money.Crear(99.5m, "USD");

        // Usar cultura invariante para evitar diferencias por separador decimal (. vs ,)
        money.ToString().Should().Be(
            $"{99.5m.ToString(System.Globalization.CultureInfo.CurrentCulture)} USD");
    }
}
