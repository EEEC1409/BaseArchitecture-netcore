using Company.NameProject.Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace Company.NameProject.Application.Tests.Common.Behaviors;

// Tipo público requerido para que Moq/Castle DynamicProxy pueda crear el proxy genérico
public record TestValidationRequest(string Valor) : IRequest<string>;

public class ValidationBehaviorTests
{
    private static RequestHandlerDelegate<string> NextHandler(string resultado = "ok")
        => _ => Task.FromResult(resultado);

    // ─── Sin validadores ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SinValidadores_LlamaSiguientePipeline()
    {
        var behavior = new ValidationBehavior<TestValidationRequest, string>(
            Enumerable.Empty<IValidator<TestValidationRequest>>());

        var result = await behavior.Handle(
            new TestValidationRequest("valor"), NextHandler("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    // ─── Con validadores que pasan ──────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidadoresOk_LlamaSiguientePipeline()
    {
        var validatorMock = new Mock<IValidator<TestValidationRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestValidationRequest, string>(
            new[] { validatorMock.Object });

        var result = await behavior.Handle(
            new TestValidationRequest("valor"), NextHandler("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    // ─── Con validadores que fallan ─────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidadoresConErrores_LanzaValidationException()
    {
        var failures = new List<ValidationFailure>
        {
            new("Valor", "El valor es requerido"),
            new("Valor", "El valor debe tener mínimo 3 caracteres")
        };

        var validatorMock = new Mock<IValidator<TestValidationRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestValidationRequest, string>(
            new[] { validatorMock.Object });

        var act = async () => await behavior.Handle(
            new TestValidationRequest(""), NextHandler(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 2);
    }

    [Fact]
    public async Task Handle_MultiplesValidadoresConErrores_AcumulaTodosLosErrores()
    {
        var validator1 = new Mock<IValidator<TestValidationRequest>>();
        validator1
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Valor", "Error v1") }));

        var validator2 = new Mock<IValidator<TestValidationRequest>>();
        validator2
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Valor", "Error v2") }));

        var behavior = new ValidationBehavior<TestValidationRequest, string>(
            new[] { validator1.Object, validator2.Object });

        var act = async () => await behavior.Handle(
            new TestValidationRequest(""), NextHandler(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 2);
    }
}
