using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoConfirm;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoReverse;
using Plugin.InboundMigoProcess.Application.Interfaces;
using Plugin.InboundMigoProcess.Application.DTO;
using Plugin.InboundMigoProcess.Application.Services;
using Plugin.InboundMigoProcess.Application.Audit.Interface;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

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
        var transactionRepository = new Mock<IInboundTransactionRepository>();
        transactionRepository
            .Setup(x => x.ObterPorTransactionIdAsync("tx-reverse-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InboundTransactionRecordDto
            {
                TransactionId = Guid.NewGuid(),
                TransactionType = "inbound.sync.inbound-delivery.confirm",
                SubscriptionId = 1,
                ResultRaw = "{}"
            });

        var useCase = new ProcessInboundMigoReverseUseCase(
            transactionRepository.Object,
            Mock.Of<IInbNfeRepository>(),
            Mock.Of<ITagManagerApiClient>(),
            Mock.Of<ITransactionResultMapper>(),
            Mock.Of<ISapMessageEvaluator>(),
            Mock.Of<ITagValueBuilder>(),
            Mock.Of<IAuditAppService>(),
            Mock.Of<INfeEntradaEventoApiClient>(),
            NullLogger<ProcessInboundMigoReverseUseCase>.Instance);

        var output = await useCase.ExecuteAsync(new ProcessInboundMigoReverseInput("tx-reverse-001"), CancellationToken.None);

        Assert.That(useCase.TransactionType, Is.EqualTo("inbound.sync.migo.reverse"));
        Assert.That(output.Sucesso, Is.True);
        Assert.That(output.Mensagem, Does.Contain("ignorado"));
    }
}
