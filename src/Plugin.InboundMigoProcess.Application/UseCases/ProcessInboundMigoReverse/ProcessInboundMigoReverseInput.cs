using Plugin.InboundMigoProcess.Application.UseCases;

namespace Plugin.InboundMigoProcess.Application.UseCases.ProcessInboundMigoReverse;

public sealed record ProcessInboundMigoReverseInput(string TransactionId) : UseCaseInput(TransactionId);
