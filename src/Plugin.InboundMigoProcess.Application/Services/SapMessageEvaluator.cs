using Plugin.InboundMigoProcess.Application.DTO;

namespace Plugin.InboundMigoProcess.Application.Services;

public interface ISapMessageEvaluator
{
    bool PossuiErro(List<SapReturnMessageDto> messages);
    bool PossuiSucesso(List<SapReturnMessageDto> messages);
    bool EhIdempotenciaJaEstornado(List<SapReturnMessageDto> messages);
}

public sealed class SapMessageEvaluator : ISapMessageEvaluator
{
    public bool PossuiErro(List<SapReturnMessageDto> messages)
        => messages.Any(m => string.Equals(m.Type, "E", StringComparison.OrdinalIgnoreCase));

    public bool PossuiSucesso(List<SapReturnMessageDto> messages)
        => messages.Any(m => string.Equals(m.Type, "S", StringComparison.OrdinalIgnoreCase));

    public bool EhIdempotenciaJaEstornado(List<SapReturnMessageDto> messages)
    {
        return messages.Any(m =>
            string.Equals(m.Type, "E", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(m.Message) &&
            m.Message.Contains("já estornado", StringComparison.OrdinalIgnoreCase));
    }
}
