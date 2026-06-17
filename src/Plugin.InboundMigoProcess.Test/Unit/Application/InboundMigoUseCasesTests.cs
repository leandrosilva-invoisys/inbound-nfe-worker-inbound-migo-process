using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoConfirm;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoReverse;

namespace Plugin.InboundMigoProcess.Test.Unit.Application;

public class InboundMigoUseCasesTests
{
    [Test]
    public async Task ConfirmUseCase_DeveRetornarSucessoComMensagemDeStub()
    {
        var useCase = new ProcessInboundMigoConfirmUseCase();

        var output = await useCase.ExecuteAsync(new ProcessInboundMigoConfirmInput("tx-confirm-001"), CancellationToken.None);

        Assert.That(useCase.TransactionType, Is.EqualTo("inbound.sync.inbound-delivery.confirm"));
        Assert.That(output.Sucesso, Is.True);
        Assert.That(output.Mensagem, Does.Contain("inicializado sem lógica de negócio"));
    }

    [Test]
    public async Task ReverseUseCase_DeveRetornarSucessoComMensagemDeStub()
    {
        var useCase = new ProcessInboundMigoReverseUseCase();

        var output = await useCase.ExecuteAsync(new ProcessInboundMigoReverseInput("tx-reverse-001"), CancellationToken.None);

        Assert.That(useCase.TransactionType, Is.EqualTo("inbound.sync.migo.reverse"));
        Assert.That(output.Sucesso, Is.True);
        Assert.That(output.Mensagem, Does.Contain("inicializado sem lógica de negócio"));
    }
}
