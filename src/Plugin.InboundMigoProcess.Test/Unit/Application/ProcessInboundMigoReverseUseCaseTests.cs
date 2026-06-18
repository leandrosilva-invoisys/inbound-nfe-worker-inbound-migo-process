using Plugin.InboundMigoProcess.Application.Audit.Interface;
using Plugin.InboundMigoProcess.Application.DTO;
using Plugin.InboundMigoProcess.Application.Interfaces;
using Plugin.InboundMigoProcess.Application.Services;
using Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoReverse;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Plugin.InboundMigoProcess.Test.Unit.Application;

[TestFixture]
public class ProcessInboundMigoReverseUseCaseTests
{
    private Mock<IInboundTransactionRepository> _transactionRepository = null!;
    private Mock<IInbNfeRepository> _inbNfeRepository = null!;
    private Mock<ITagManagerApiClient> _tagManagerApiClient = null!;
    private Mock<IAuditAppService> _auditAppService = null!;
    private Mock<INfeEntradaEventoApiClient> _nfeEntradaEventoApiClient = null!;
    private ProcessInboundMigoReverseUseCase _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _transactionRepository = new Mock<IInboundTransactionRepository>();
        _inbNfeRepository = new Mock<IInbNfeRepository>();
        _tagManagerApiClient = new Mock<ITagManagerApiClient>();
        _auditAppService = new Mock<IAuditAppService>();
        _nfeEntradaEventoApiClient = new Mock<INfeEntradaEventoApiClient>();

        _useCase = new ProcessInboundMigoReverseUseCase(
            _transactionRepository.Object,
            _inbNfeRepository.Object,
            _tagManagerApiClient.Object,
            new TransactionResultMapper(),
            new SapMessageEvaluator(),
            new TagValueBuilder(),
            _auditAppService.Object,
            _nfeEntradaEventoApiClient.Object,
            NullLogger<ProcessInboundMigoReverseUseCase>.Instance);
    }

    [Test]
    public async Task ExecuteAsync_TipoSucesso_DeveAtualizarTagsComSucesso()
    {
        SetupTransacao("""
            {
              "RETURN":[{"TYPE":"S","MESSAGE":"Documento Material estornado com sucesso","MESSAGEV1":"5000999999","MESSAGEV2":"2026"}]
            }
            """);

        var output = await _useCase.ExecuteAsync(new ProcessInboundMigoReverseInput("d5f71d0b-5d56-4f1b-8c44-450dbcc51e1a"), CancellationToken.None);

        Assert.That(output.Sucesso, Is.True);
        _tagManagerApiClient.Verify(x => x.AtualizarTagAsync(
            It.Is<TagManagerRequestUpdate>(r => r.Id == 20 && r.Status == 2),
            It.IsAny<CancellationToken>()), Times.Once);
        _tagManagerApiClient.Verify(x => x.AtualizarTagAsync(
            It.Is<TagManagerRequestUpdate>(r => r.Id == 10 && r.Status == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_IdempotenciaJaEstornado_DeveTratarComoSucesso()
    {
        SetupTransacao("""
            {
              "RETURN":[{"TYPE":"E","MESSAGE":"Documento Material já estornado","MESSAGEV1":"5000123456","MESSAGEV2":"2026"}]
            }
            """);

        var output = await _useCase.ExecuteAsync(new ProcessInboundMigoReverseInput("d5f71d0b-5d56-4f1b-8c44-450dbcc51e1a"), CancellationToken.None);

        Assert.That(output.Sucesso, Is.True);
        _tagManagerApiClient.Verify(x => x.AtualizarTagAsync(
            It.Is<TagManagerRequestUpdate>(r => r.Id == 20 && r.Status == 2),
            It.IsAny<CancellationToken>()), Times.Once);
        _tagManagerApiClient.Verify(x => x.AtualizarTagAsync(
            It.Is<TagManagerRequestUpdate>(r => r.Id == 10 && r.Status == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ErroGenerico_DeveManterTagInboundMigoESinalizarErro()
    {
        SetupTransacao("""
            {
              "RETURN":[{"TYPE":"E","MESSAGE":"Período fiscal fechado","MESSAGEV1":"5000123456","MESSAGEV2":"2026"}]
            }
            """);

        var output = await _useCase.ExecuteAsync(new ProcessInboundMigoReverseInput("d5f71d0b-5d56-4f1b-8c44-450dbcc51e1a"), CancellationToken.None);

        Assert.That(output.Sucesso, Is.True);
        _tagManagerApiClient.Verify(x => x.AtualizarTagAsync(
            It.Is<TagManagerRequestUpdate>(r => r.Id == 20 && r.Status == 3),
            It.IsAny<CancellationToken>()), Times.Once);
        _tagManagerApiClient.Verify(x => x.AtualizarTagAsync(
            It.Is<TagManagerRequestUpdate>(r => r.Id == 10 && r.Status == 3),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupTransacao(string resultRaw)
    {
        _transactionRepository.Setup(x => x.ObterPorTransactionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InboundTransactionRecordDto
            {
                TransactionId = Guid.NewGuid(),
                TransactionType = "inbound.sync.migo.reverse",
                SubscriptionId = 1,
                DocId = 100,
                OriginatorService = "tester",
                TagId = 20,
                ResultRaw = resultRaw
            });

        _tagManagerApiClient.Setup(x => x.ObterTagAsync(1, 100, "inbound.migo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TagManagerTagDto
            {
                Id = 10,
                Status = 1,
                TagValue = """{"materialdocument":"5000123456","matdocumentyear":"2026"}"""
            });

        _tagManagerApiClient.Setup(x => x.ObterTagAsync(1, 100, "inbound.migo.cancelamento", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TagManagerTagDto
            {
                Id = 20,
                Status = 1,
                TagValue = """{"transactionId":"abc"}"""
            });

        _tagManagerApiClient.Setup(x => x.AtualizarTagAsync(It.IsAny<TagManagerRequestUpdate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _inbNfeRepository.Setup(x => x.ObterDocId20Async(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)null);
    }
}
