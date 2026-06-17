using Plugin.InboundMigoProcess.Application.UseCases;

namespace Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoConfirm;

public sealed record ProcessInboundMigoConfirmInput(string TransactionId) : UseCaseInput(TransactionId);
