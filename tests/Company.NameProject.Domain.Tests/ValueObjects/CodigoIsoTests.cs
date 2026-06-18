using Company.NameProject.Domain.Common;
using Company.NameProject.Domain.ValueObjects;
using FluentAssertions;

namespace Company.NameProject.Domain.Tests.ValueObjects;

public class CodigoIsoTests
{
    // ─── Crear — casos válidos ──────────────────────────────────────────────

    [Fact]
    public void Crear_ConCodigoValido_RetornaInstancia()
    {
        var codigo = CodigoIso.Crear("pe");

        codigo.Value.Should().Be("PE");
    }

    [Fact]
    public void Crear_NormalizaAMayusculas()
    {
        var codigo = CodigoIso.Crear("us");

        codigo.Value.Should().Be("US");
    }

    [Fact]
    public void Crear_EliminaEspaciosExternos()
    {
        var codigo = CodigoIso.Crear(" US ");

        codigo.Value.Should().Be("US");
    }

    // ─── Crear — casos inválidos ────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConCodigoVacioONulo_LanzaDomainException(string? valor)
    {
        var act = () => CodigoIso.Crear(valor!);

        act.Should().Throw<DomainException>().WithMessage("Código ISO requerido");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("ABC")]
    [InlineData("PERU")]
    public void Crear_ConLongitudDistintaDe2_LanzaDomainException(string valor)
    {
        var act = () => CodigoIso.Crear(valor);

        act.Should().Throw<DomainException>()
            .WithMessage("*2 caracteres*");
    }

    // ─── Igualdad ───────────────────────────────────────────────────────────

    [Fact]
    public void DosCodigos_ConMismoValor_SonIguales()
    {
        var a = CodigoIso.Crear("PE");
        var b = CodigoIso.Crear("pe");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void DosCodigos_ConDistintoValor_NoSonIguales()
    {
        var a = CodigoIso.Crear("PE");
        var b = CodigoIso.Crear("US");

        (a != b).Should().BeTrue();
    }

    [Fact]
    public void ToString_RetornaElValor()
    {
        var codigo = CodigoIso.Crear("PE");

        codigo.ToString().Should().Be("PE");
    }
}
