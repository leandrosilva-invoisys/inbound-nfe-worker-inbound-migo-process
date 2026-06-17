using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigo;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoConfirm;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoReverse;

namespace Plugin.InboundMigoProcess.Test.Unit.Application;

public class InboundMigoUseCaseFactoryTests
{
    [Test]
    public void Resolve_DeveEncontrarUseCasePorTransactionType_IgnoreCase()
    {
        var useCases = new IProcessInboundMigoUseCase[]
        {
            new ProcessInboundMigoConfirmUseCase(),
            new ProcessInboundMigoReverseUseCase()
        };

        var factory = new InboundMigoUseCaseFactory(useCases);

        var confirmUseCase = factory.Resolve("INBOUND.SYNC.INBOUND-DELIVERY.CONFIRM");
        var reverseUseCase = factory.Resolve("inbound.sync.migo.reverse");

        Assert.That(confirmUseCase, Is.TypeOf<ProcessInboundMigoConfirmUseCase>());
        Assert.That(reverseUseCase, Is.TypeOf<ProcessInboundMigoReverseUseCase>());
    }

    [Test]
    public void Resolve_DeveRetornarNulo_QuandoTransactionTypeNaoExiste()
    {
        var useCases = new IProcessInboundMigoUseCase[]
        {
            new ProcessInboundMigoConfirmUseCase(),
            new ProcessInboundMigoReverseUseCase()
        };

        var factory = new InboundMigoUseCaseFactory(useCases);

        var resolved = factory.Resolve("nao.existe");

        Assert.That(resolved, Is.Null);
    }
}
