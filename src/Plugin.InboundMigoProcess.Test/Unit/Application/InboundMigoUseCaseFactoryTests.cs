using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoConfirm;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoReverse;
using Plugin.InboundMigoProcess.Application.UseCases;

namespace Plugin.InboundMigoProcess.Test.Unit.Application;

public class InboundMigoUseCaseFactoryTests
{
    private sealed class FakeUseCase(string transactionType) : IProcessInboundMigoUseCase
    {
        public string TransactionType { get; } = transactionType;

        public Task<UseCaseOutput> ExecuteAsync(string transactionId, CancellationToken cancellationToken)
            => Task.FromResult<UseCaseOutput>(new ProcessInboundMigoConfirmOutput { Sucesso = true });
    }

    [Test]
    public void Resolve_DeveEncontrarUseCasePorTransactionType_IgnoreCase()
    {
        var useCases = new IProcessInboundMigoUseCase[]
        {
            new FakeUseCase(ProcessInboundMigoConfirmUseCase.TransactionTypeConfirm),
            new FakeUseCase(ProcessInboundMigoReverseUseCase.TransactionTypeReverse)
        };

        var factory = new InboundMigoUseCaseFactory(useCases);

        var confirmUseCase = factory.Resolve("INBOUND.SYNC.INBOUND-DELIVERY.CONFIRM");
        var reverseUseCase = factory.Resolve("inbound.sync.migo.reverse");

        Assert.That(confirmUseCase, Is.Not.Null);
        Assert.That(reverseUseCase, Is.Not.Null);
    }

    [Test]
    public void Resolve_DeveRetornarNulo_QuandoTransactionTypeNaoExiste()
    {
        var useCases = new IProcessInboundMigoUseCase[]
        {
            new FakeUseCase(ProcessInboundMigoConfirmUseCase.TransactionTypeConfirm),
            new FakeUseCase(ProcessInboundMigoReverseUseCase.TransactionTypeReverse)
        };

        var factory = new InboundMigoUseCaseFactory(useCases);

        var resolved = factory.Resolve("nao.existe");

        Assert.That(resolved, Is.Null);
    }
}
