using Company.NameProject.Domain.Common;
using Company.NameProject.Domain.ValueObjects;
using FluentAssertions;

namespace Company.NameProject.Domain.Tests.ValueObjects;

public class EmailTests
{
    // ─── Crear — casos válidos ──────────────────────────────────────────────

    [Fact]
    public void Crear_ConEmailValido_RetornaInstancia()
    {
        // Arrange
        var raw = "Usuario@Ejemplo.com";

        // Act
        var email = Email.Crear(raw);

        // Assert
        email.Value.Should().Be("usuario@ejemplo.com");
    }

    [Fact]
    public void Crear_NormalizaAMinusculas()
    {
        var email = Email.Crear("UPPER@DOMAIN.COM");

        email.Value.Should().Be("upper@domain.com");
    }

    [Fact]
    public void Crear_EliminaEspaciosExternos()
    {
        var email = Email.Crear("  test@test.com  ");

        email.Value.Should().Be("test@test.com");
    }

    // ─── Crear — casos inválidos ────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConEmailVacioONulo_LanzaDomainException(string? valor)
    {
        // Act
        var act = () => Email.Crear(valor!);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Email requerido");
    }

    [Fact]
    public void Crear_SinArroba_LanzaDomainException()
    {
        var act = () => Email.Crear("invalido.com");

        act.Should().Throw<DomainException>().WithMessage("Email inválido");
    }

    // ─── Igualdad ───────────────────────────────────────────────────────────

    [Fact]
    public void DosEmails_ConMismoValor_SonIguales()
    {
        var a = Email.Crear("a@b.com");
        var b = Email.Crear("a@b.com");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void DosEmails_ConDistintoValor_NoSonIguales()
    {
        var a = Email.Crear("a@b.com");
        var b = Email.Crear("c@d.com");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void ToString_RetornaElValor()
    {
        var email = Email.Crear("test@test.com");

        email.ToString().Should().Be("test@test.com");
    }
}
